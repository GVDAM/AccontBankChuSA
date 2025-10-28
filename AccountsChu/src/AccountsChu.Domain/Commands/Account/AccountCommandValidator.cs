using FluentValidation;

namespace AccountsChu.Domain.Commands.Account
{
    public class AccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        public AccountCommandValidator()
        {
            RuleFor(x => x.agency)
                .InclusiveBetween((short)1, (short)99)
                .WithMessage("Número da agência deve ser entre 1 e 99");

            RuleFor(x => x.number)
                .InclusiveBetween(1, 99)
                .WithMessage("Número da conta deve ser entre 1 e 99");

            RuleFor(x => x.balance)
                .Must(x => x >= 10000)
                .WithMessage("Valor mínimo para abrir uma conta é de R$ 10.000,00");
        }
    }
}
