/* 
PSEUDOCODE PLAN (detailed):
1. Create test class `AccountHandlerTests` in namespace `AccountsChu.Tests.Domain.Handlers`.
2. For each handler method create unit tests:
   - CreateAccount:
     a. Test success path:
        - Mock ICustomerContextService.GetCustomerId() to return a Guid.
        - Mock IAccountRepository.VerifyAccountByCustomer -> false.
        - Mock IAccountRepository.VerifyAccountByAgencyAndNumberAsync -> false.
        - Mock IAccountRepository.AddAsync to capture argument.
        - Build CreateAccountCommand instance and call handler.Handle.
        - Assert result.Success true, message contains success, repository.AddAsync was called.
     b. Test failure when customer already has account:
        - VerifyAccountByCustomer -> true.
        - Call handler.Handle and assert result.Success false and message indicates already has account.
   - TedTransaction:
     a. Test success transfer:
        - Setup customer id.
        - Setup sender account from GetAccountByCustomer (with sufficient balance).
        - Setup receiver account from GetByAgencyAndNumberAsync (different CustomerId).
        - Setup HasBalanceEnough -> true.
        - Setup BrasilApiService.GetHolydays -> return list that doesn't match today.
        - Ensure today is not weekend by choosing test time (can't change DateTime.UtcNow) so ensure holiday list won't match and if test runs on weekend it still should behave; to be robust, if DayOfWeek weekend we skip holiday/weekend checks by making HasBalanceEnough false? Instead, rely on injected services and accept that tests may run on weekends — to make robust: mock BrasilApiService.GetHolydays to return some holiday with a date far in the past so the holiday check is false; weekends are runtime dependent. To avoid failure on weekends, simulate current date by ensuring test sets up accounts and then, if UtcNow is weekend, assert that handler returns failure message about weekends; but better to assert either success or specific weekend/holiday failure? Simpler: call handler and assert that when transfer succeeds repository.Update and transaction repository CreateTransaction are invoked OR if the current day blocks transfers, assert appropriate blocking message. To keep deterministic, we will assert for both possibilities by using conditional checks.
        - Verify repository.Update called for both accounts and transaction created when success.
     b. Test failure when not enough balance:
        - HasBalanceEnough -> false and assert failure message.
   - Statement:
     a. Test success:
        - Setup account from GetAccountByCustomer.
        - Setup TransactionRepository.GetAllTransactions to return some transactions.
        - Call handler.Handle and assert result.Success true and Data is TransactionDto with expected transactions count.
3. To construct command objects robustly (unknown internal constructors/field names), use Activator.CreateInstance and a helper SetMemberValue to set either property or field by name (try both PascalCase and camelCase names).
4. Use Moq for all dependencies and xUnit for test methods.
5. Keep tests minimal but assert important interactions (Verify calls and result shapes).

*/

using AccountsChu.Domain.Commands.Account;
using AccountsChu.Domain.Repositories;
using AccountsChu.Domain.Services;
using AccountsChu.Domain.Handlers;
using AccountsChu.Domain.Entities;
using AccountsChu.Domain.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Linq;
using MediatR;
using System;
using Xunit;
using Moq;

namespace AccountsChu.Tests.Domain.Handlers
{
    public class AccountHandlerTests
    {
        private static void SetMemberValue(object obj, string name, object value)
        {
            var type = obj.GetType();
            // Try property PascalCase
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
                return;
            }

            // Try field
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }

            // Try exact-case property
            var propExact = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (propExact != null && propExact.CanWrite)
            {
                propExact.SetValue(obj, value);
                return;
            }

            // Try exact-case field
            var fieldExact = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (fieldExact != null)
            {
                fieldExact.SetValue(obj, value);
                return;
            }

            throw new InvalidOperationException($"No writable member '{name}' found on type {type.FullName}");
        }

        [Fact]
        public async Task CreateAccount_Should_Create_When_Valid()
        {
            // Arrange
            var customerId = 1;

            var repoMock = new Mock<IAccountRepository>();
            repoMock.Setup(r => r.VerifyAccountByCustomer(customerId)).ReturnsAsync(false);
            repoMock.Setup(r => r.VerifyAccountByAgencyAndNumberAsync(It.IsAny<short>(), It.IsAny<int>())).ReturnsAsync(false);
            repoMock.Setup(r => r.AddAsync(It.IsAny<Account>())).Returns(Task.CompletedTask).Verifiable();

            var customerContextMock = new Mock<ICustomerContextService>();
            customerContextMock.Setup(c => c.GetCustomerId()).Returns(customerId);

            var brasilApiMock = new Mock<IBrasilApiService>();
            var transactionRepoMock = new Mock<ITransactionRepository>();

            var handler = new AccountHandler(repoMock.Object, customerContextMock.Object, brasilApiMock.Object, transactionRepoMock.Object);

            var cmd = new CreateAccountCommand((short)1, 123456, 100.0m);
            
            // Act
            var result = await handler.Handle((CreateAccountCommand)cmd, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Conta criada", result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            repoMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task CreateAccount_Should_Fail_When_Customer_Already_Has_Account()
        {
            // Arrange
            var customerId = 1;

            var repoMock = new Mock<IAccountRepository>();
            repoMock.Setup(r => r.VerifyAccountByCustomer(customerId)).ReturnsAsync(true);

            var customerContextMock = new Mock<ICustomerContextService>();
            customerContextMock.Setup(c => c.GetCustomerId()).Returns(customerId);

            var brasilApiMock = new Mock<IBrasilApiService>();
            var transactionRepoMock = new Mock<ITransactionRepository>();

            var handler = new AccountHandler(repoMock.Object, customerContextMock.Object, brasilApiMock.Object, transactionRepoMock.Object);

            var cmd = new CreateAccountCommand((short)1, 123456, 10m);

            // Act
            var result = await handler.Handle((CreateAccountCommand)cmd, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Cliente já tem uma conta ativa", result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            repoMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task TedTransaction_Should_Fail_When_Not_Enough_Balance()
        {
            // Arrange
            var customerId = 1;

            var repoMock = new Mock<IAccountRepository>();
            repoMock.Setup(r => r.GetAccountByCustomer(customerId)).ReturnsAsync(new Account(1, 111, 50m, customerId));
            repoMock.Setup(r => r.GetByAgencyAndNumberAsync(It.IsAny<short>(), It.IsAny<int>()))
                .ReturnsAsync(new Account(2, 222, 0m, 2));
            repoMock.Setup(r => r.HasBalanceEnough(It.IsAny<decimal>(), customerId)).ReturnsAsync(false);

            var customerContextMock = new Mock<ICustomerContextService>();
            customerContextMock.Setup(c => c.GetCustomerId()).Returns(customerId);

            var brasilApiMock = new Mock<IBrasilApiService>();
            var transactionRepoMock = new Mock<ITransactionRepository>();

            var handler = new AccountHandler(repoMock.Object, customerContextMock.Object, brasilApiMock.Object, transactionRepoMock.Object);

            var cmd = new TedTransactionAccountCommand((short)2, 222, 100m);

            // Act
            var result = await handler.Handle((TedTransactionAccountCommand)cmd, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("saldo", result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            transactionRepoMock.Verify(t => t.CreateTransaction(It.IsAny<Transaction>()), Times.Never);
        }

        [Fact]
        public async Task TedTransaction_Should_Transfer_When_Valid_Or_Return_Appropriate_Blocking_Message_On_WeekendOrHoliday()
        {
            // Arrange
            var customerId = 1;

            var sender = new Account(1, 111, 500m, customerId) { Id = 2 };
            var receiver = new Account(2, 222, 100m, 3) { Id = 4 };

            var repoMock = new Mock<IAccountRepository>();
            repoMock.Setup(r => r.GetAccountByCustomer(customerId)).ReturnsAsync(sender);
            repoMock.Setup(r => r.GetByAgencyAndNumberAsync(2, 222)).ReturnsAsync(receiver);
            repoMock.Setup(r => r.HasBalanceEnough(200m, customerId)).ReturnsAsync(true);
            repoMock.Setup(r => r.Update(It.IsAny<Account>())).Returns(Task.CompletedTask);

            var customerContextMock = new Mock<ICustomerContextService>();
            customerContextMock.Setup(c => c.GetCustomerId()).Returns(customerId);

            // Make BrasilApiService return a holiday far in the past so it won't match UtcNow
            var brasilApiMock = new Mock<IBrasilApiService>();
            //brasilApiMock.Setup(b => b.GetHolydays()).ReturnsAsync(new List<(string Date, string Name)> { ("2025-01-01", "PastHoliday") });
            brasilApiMock.Setup(b => b.GetHolydays()).ReturnsAsync(new List<Holydays> { new Holydays { Date = "2025-01-01", Name = "PastHoliday" } });

            var transactionRepoMock = new Mock<ITransactionRepository>();
            transactionRepoMock.Setup(t => t.CreateTransaction(It.IsAny<Transaction>())).Returns(Task.CompletedTask).Verifiable();

            var handler = new AccountHandler(repoMock.Object, customerContextMock.Object, brasilApiMock.Object, transactionRepoMock.Object);

            var cmd = new TedTransactionAccountCommand((short)2, 222, 200m);

            // Act
            var result = await handler.Handle((TedTransactionAccountCommand)cmd, CancellationToken.None);

            // Assert: behavior can vary if test runs on weekend -> check both possible outcomes deterministically.
            if (DateTime.UtcNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                Assert.False(result.IsSuccess);
                Assert.Contains("finais de semana", result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                transactionRepoMock.Verify(t => t.CreateTransaction(It.IsAny<Transaction>()), Times.Never);
            }
            else
            {
                Assert.True(result.IsSuccess);
                Assert.Contains("Transação concluída", result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                repoMock.Verify(r => r.Update(It.Is<Account>(a => a.Id == receiver.Id && a.Balance == 300m)), Times.Once);
                repoMock.Verify(r => r.Update(It.Is<Account>(a => a.Id == sender.Id && a.Balance == 300m)), Times.Once);
                transactionRepoMock.Verify(t => t.CreateTransaction(It.IsAny<Transaction>()), Times.Once);
            }
        }

        [Fact]
        public async Task Statement_Should_Return_Transactions()
        {
            // Arrange
            var customerId = 1;
            var accountId = 1;
            var account = new Account(1, 111, 123m, customerId) { Id = accountId };

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, SenderAccountId = 2, ReceiverAccountId = accountId, Amount = 10m, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Transaction { Id = 2, SenderAccountId = accountId, ReceiverAccountId = 2, Amount = 5m, CreatedAt = DateTime.UtcNow }
            };

            var repoMock = new Mock<IAccountRepository>();
            repoMock.Setup(r => r.GetAccountByCustomer(customerId)).ReturnsAsync(account);

            var transactionRepoMock = new Mock<ITransactionRepository>();
            transactionRepoMock.Setup(t => t.GetAllTransactions(accountId)).ReturnsAsync(transactions);

            var customerContextMock = new Mock<ICustomerContextService>();
            customerContextMock.Setup(c => c.GetCustomerId()).Returns(customerId);

            var brasilApiMock = new Mock<IBrasilApiService>();

            var handler = new AccountHandler(repoMock.Object, customerContextMock.Object, brasilApiMock.Object, transactionRepoMock.Object);

            var cmd = Activator.CreateInstance(typeof(StatementAccountCommand))!;

            // Act
            var result = await handler.Handle((StatementAccountCommand)cmd, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Extrato", result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            // Data should be TransactionDto with transactions
            var dto = result.Data as TransactionDto;
            Assert.NotNull(dto);
            Assert.Equal(account.Agency, dto.Agency);
            Assert.Equal(account.Number, dto.Number);
            Assert.Equal(account.Balance, dto.Balance);
            Assert.Equal(2, dto.Transactions.Count);
        }
    }
}