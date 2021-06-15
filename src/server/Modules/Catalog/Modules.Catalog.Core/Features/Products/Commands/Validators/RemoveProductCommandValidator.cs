using FluentValidation;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Products.Commands.Validators
{
    public class RemoveProductCommandValidator : AbstractValidator<RemoveProductCommand>
    {
        public RemoveProductCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEqual(Guid.Empty);
        }
    }
}