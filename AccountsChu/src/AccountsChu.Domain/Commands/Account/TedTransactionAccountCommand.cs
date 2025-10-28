using MediatR;

namespace AccountsChu.Domain.Commands.Account
{
    public record TedTransactionAccountCommand(Int16 agency, Int32 number, decimal amount) : IRequest<GenericCommandResult>;
}
