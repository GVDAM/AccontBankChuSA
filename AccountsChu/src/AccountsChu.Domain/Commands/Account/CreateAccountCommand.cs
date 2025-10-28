using MediatR;

namespace AccountsChu.Domain.Commands.Account
{
    public record CreateAccountCommand(Int16 Agency, Int32 Number, decimal Balance) : IRequest<GenericCommandResult>;
}
