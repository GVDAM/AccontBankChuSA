using FluentValidation;

namespace AccountsChu.Domain.Commands.Customer
{
    public class CustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CustomerCommandValidator() 
        {
            RuleFor(x => x.email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("É obrigatório um email válido");

            RuleFor(x => x.name)
                .NotEmpty()
                .WithMessage("Nome é obrigatório");

            RuleFor(x => x.password)
                .NotEmpty().WithMessage("Senha é obrigatório kkkk")
                .MinimumLength(3).WithMessage("Senha deve ter no Mínimo 3 caracteres");
        }
    }
}
