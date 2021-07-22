using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Shared.Core.Features.ExtendedAttributes.Commands.Validators;
using Microsoft.Extensions.Localization;
using System;

namespace FluentPOS.Modules.Identity.Core.Features.ExtendedAttributes.Validators
{
    public class RemoveRoleExtendedAttributeCommandValidator : RemoveExtendedAttributeCommandValidator<Guid, FluentRole>
    {
        public RemoveRoleExtendedAttributeCommandValidator(IStringLocalizer<RemoveRoleExtendedAttributeCommandValidator> localizer) : base(localizer)
        {
            // you can override the validation rules here
        }
    }
}