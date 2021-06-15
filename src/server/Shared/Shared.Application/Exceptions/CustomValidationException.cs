using System.Collections.Generic;

namespace FluentPOS.Shared.Application.Exceptions
{
    public class CustomValidationException : CustomException
    {
        public CustomValidationException(List<string> errors) : base("One or more validation failures have occurred.", errors)
        {
        }
    }
}