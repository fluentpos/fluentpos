using FluentValidation;

namespace FluentPOS.Modules.Catalog.Core.Features.Products.Commands.Validators
{
    public class RegisterProductCommandValidator : AbstractValidator<RegisterProductCommand>
    {
        public RegisterProductCommandValidator()
        {
            RuleFor(c => c.Name)
               .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.")
               .Length(2, 150).WithMessage("The {PropertyName} property must have between 2 and 150 characters.");
            RuleFor(c => c.Detail)
               .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.")
               .Length(2, 150).WithMessage("The {PropertyName} property must have between 2 and 150 characters.");
            RuleFor(c => c.BrandId)
                .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.");
            RuleFor(c => c.CategoryId)
                .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.");
            RuleFor(c => c.Price)
                .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.");
            RuleFor(c => c.Cost)
                .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.");
            RuleFor(c => c.BarcodeSymbology)
                .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.")
                .Length(5, 150).WithMessage("The {PropertyName} property must have between 5 and 150 characters.");
        }
    }
}