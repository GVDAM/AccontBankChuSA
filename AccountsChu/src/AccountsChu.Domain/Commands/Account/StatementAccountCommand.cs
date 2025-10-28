using MediatR;

namespace AccountsChu.Domain.Commands.Account
{
    public record StatementAccountCommand() : IRequest<GenericCommandResult>;
}
