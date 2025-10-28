using MediatR;

namespace AccountsChu.Domain.Commands.Account
{
    public record CreateAccountCommand(Int16 agency, Int32 number, decimal balance) : IRequest<GenericCommandResult>;
}
