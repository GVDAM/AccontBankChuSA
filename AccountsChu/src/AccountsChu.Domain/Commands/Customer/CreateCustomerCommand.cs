using MediatR;

namespace AccountsChu.Domain.Commands.Customer
{
    public record CreateCustomerCommand(string name, string email, string password) : IRequest<GenericCommandResult>;
}
