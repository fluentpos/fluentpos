using FluentValidation;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Categories.Commands.Validators
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEqual(Guid.Empty);
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.")
                .Length(2, 150).WithMessage("The {PropertyName} property must have between 2 and 150 characters.");
            RuleFor(c => c.Detail)
               .NotEmpty().WithMessage("The {PropertyName} property cannot be empty.")
               .Length(2, 150).WithMessage("The {PropertyName} property must have between 2 and 150 characters.");
        }
    }
}