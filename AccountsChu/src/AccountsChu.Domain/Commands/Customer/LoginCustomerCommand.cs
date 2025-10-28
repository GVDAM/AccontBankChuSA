using MediatR;

namespace AccountsChu.Domain.Commands.Customer
{
    public record LoginCustomerCommand(string email, string password) : IRequest<GenericCommandResult>;
}
