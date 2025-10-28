using AccountsChu.Domain.Commands.Account;
using AccountsChu.Domain.Repositories;
using AccountsChu.Domain.Commands;
using AccountsChu.Domain.Entities;
using AccountsChu.Domain.Services;
using MediatR;

namespace AccountsChu.Domain.Handlers
{
    public class AccountHandler :
        IRequestHandler<CreateAccountCommand, GenericCommandResult>,
        IRequestHandler<TedTransactionAccountCommand, GenericCommandResult>,
        IRequestHandler<StatementAccountCommand, GenericCommandResult>

    {

        private readonly ICustomerContextService _customerContextService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBrasilApiService _brasilApiService;
        private readonly IAccountRepository _repository;

        public AccountHandler(
            IAccountRepository repository,
            ICustomerContextService customerContextService,
            IBrasilApiService brasilApiService,
            ITransactionRepository transactionRepository)
        {
            _repository = repository;
            _customerContextService = customerContextService;
            _brasilApiService = brasilApiService;
            _transactionRepository = transactionRepository;
        }


        public async Task<GenericCommandResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var validator = new AccountCommandValidator();
                var results = validator.Validate(request);

                if (!results.IsValid)
                {
                    var errors = new List<string>();
                    foreach (var failure in results.Errors)
                    {
                        errors.Add(failure.ErrorMessage);
                    }

                    return new GenericCommandResult(false, "Erro nos dados fornecidos", errors);
                }

                var customerId = _customerContextService.GetCustomerId()!.Value;

                var userAlreadyRegistered = await _repository.VerifyAccountByCustomer(customerId);

                if (userAlreadyRegistered)
                    return new GenericCommandResult(false, "Cliente já tem uma conta ativa!");

                var accountAlreadyCreated = await _repository.VerifyAccountByAgencyAndNumberAsync(request.agency, request.number);

                if (accountAlreadyCreated)
                    return new GenericCommandResult(false, "Esta conta já existe!");

                var account = new Account(request.agency, request.number, request.balance, customerId);

                await _repository.AddAsync(account);

                return new GenericCommandResult(true, "Conta criada com sucesso!", account);
            }
            catch (Exception ex)
            {
                return new GenericCommandResult(false, $"Erro ao criar conta: {ex.Message}");
            }            
        }

        public async Task<GenericCommandResult> Handle(TedTransactionAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var customerId = _customerContextService.GetCustomerId()!.Value;

                var accountCustomerLogged = await _repository.GetAccountByCustomer(customerId);
                if (accountCustomerLogged == null)
                    return new GenericCommandResult(false, "Você precisa ter uma conta para realizar uma transferência!");

                var account = await _repository.GetByAgencyAndNumberAsync(request.agency, request.number);
                if (account == null)
                    return new GenericCommandResult(false, "Conta para transferência não existe!");

                if (account.CustomerId == customerId)
                    return new GenericCommandResult(false, "Não é possível fazer uma transferência para você mesmo =)");

                var hasBalanceEnough = await _repository.HasBalanceEnough(request.amount, customerId);
                if (!hasBalanceEnough)
                    return new GenericCommandResult(false, "Não há saldo o suficiente para concluir a transação!");

                var holydays = await _brasilApiService.GetHolydays();
                if (holydays.Any(x => DateOnly.Parse(x.Date).ToString() == DateTime.UtcNow.ToString()))
                    return new GenericCommandResult(false, "Não pode realizar transferências em feriados!");

                if (DateTime.UtcNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    return new GenericCommandResult(false, "Não é possível realizar transferências aos finais de semana");

                // Atualizar saldo da conta destino.
                account.Balance += request.amount;
                await _repository.Update(account);

                // Atualizar saldo do usuário solicitante
                accountCustomerLogged.Balance -= request.amount;
                await _repository.Update(accountCustomerLogged);

                // Salvar Nova Transação
                var transaction = new Transaction()
                {
                    SenderAccountId = accountCustomerLogged.Id,
                    ReceiverAccountId = account.Id,
                    Amount = request.amount,
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepository.CreateTransaction(transaction);

                return new GenericCommandResult(true, "Transação concluída com sucesso!");

            }
            catch (Exception ex)
            {
                return new GenericCommandResult(false, ex.Message);
            }
        }

        public async Task<GenericCommandResult> Handle(StatementAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var account = await _repository.GetAccountByCustomer(_customerContextService.GetCustomerId()!.Value);

                if (account == null)
                    return new GenericCommandResult(true, "nada a mostrar no extrato");

                var transactions = await _transactionRepository.GetAllTransactions(account.Id);

                var response = new TransactionDto()
                {
                    Id = account.Id,
                    Agency = account.Agency,
                    Number = account.Number,
                    Balance = account.Balance,
                };


                foreach (var transaction in transactions)
                {
                    response.Transactions.Add(new() 
                    { 
                        Amount = transaction.Amount,
                        BalanceInOut = transaction.ReceiverAccountId == account.Id ? "Entrada" : "Saída",
                        CreatedAt = transaction.CreatedAt,
                    });
                }


                return new GenericCommandResult(true, "Extrato obtido com sucesso!", response);
            }
            catch (Exception ex)
            {
                return new GenericCommandResult(false, ex.Message);
            }
        }
    }
}
