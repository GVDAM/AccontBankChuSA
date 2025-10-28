using AccountsChu.Domain.Commands;
using AccountsChu.Domain.Commands.Account;
using AccountsChu.Domain.Commands.Customer;
using AccountsChu.Domain.Entities;
using AccountsChu.Domain.Repositories;
using AccountsChu.Domain.Services;
using MediatR;

namespace AccountsChu.Domain.Handlers
{
    public class CustomerHandler :
        IRequestHandler<LoginCustomerCommand, GenericCommandResult>,
        IRequestHandler<CreateCustomerCommand, GenericCommandResult>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITokenService _tokenService;

        public CustomerHandler(ICustomerRepository customerRepository,
            ITokenService tokenService)
        {
            _customerRepository = customerRepository;
            _tokenService = tokenService;
        }


        public async Task<GenericCommandResult> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var validator = new CustomerCommandValidator();
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
                 
                var exists = await _customerRepository.IsAlreadyCreated(request.email);

                if (exists)
                    return new GenericCommandResult(false, "Usuário já existente");

                var newCustomer = new Customer
                {
                    Email = request.email,
                    Name = request.name,
                    Password = request.password,
                };

                await _customerRepository.CreateCustomer(newCustomer);

                return new GenericCommandResult(true, "Cliente criado com sucesso!");
            }
            catch (Exception ex)
            {
                return new GenericCommandResult(false, ex.Message);
            }
        }


        public async Task<GenericCommandResult> Handle(LoginCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerLogin(request.email, request.password);

                if (customer == null)
                    return new GenericCommandResult(false, "Usuário ou senha incorretos!");

                var token = _tokenService.GenerateToken(customer);

                return new GenericCommandResult(true, "Autenticado com sucesso!", new
                {
                    Email = customer.Email,
                    Name = customer.Name,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return new GenericCommandResult(false, ex.Message);
            }
        }        
    }
}
