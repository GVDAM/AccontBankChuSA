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

            var brasilApiMock = new Mock<IBrasilApiService>();
            brasilApiMock.Setup(b => b.GetHolydays()).ReturnsAsync(new List<Holydays> { new Holydays { Date = "2025-01-01", Name = "PastHoliday" } });

            var transactionRepoMock = new Mock<ITransactionRepository>();
            transactionRepoMock.Setup(t => t.CreateTransaction(It.IsAny<Transaction>())).Returns(Task.CompletedTask).Verifiable();

            var handler = new AccountHandler(repoMock.Object, customerContextMock.Object, brasilApiMock.Object, transactionRepoMock.Object);

            var cmd = new TedTransactionAccountCommand((short)2, 222, 200m);

            // Act
            var result = await handler.Handle((TedTransactionAccountCommand)cmd, CancellationToken.None);

            // Assert
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

            var dto = result.Data as TransactionDto;
            Assert.NotNull(dto);
            Assert.Equal(account.Agency, dto.Agency);
            Assert.Equal(account.Number, dto.Number);
            Assert.Equal(account.Balance, dto.Balance);
            Assert.Equal(2, dto.Transactions.Count);
        }
    }
}