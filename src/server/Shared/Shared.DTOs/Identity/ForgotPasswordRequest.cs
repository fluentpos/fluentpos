using System.ComponentModel.DataAnnotations;

namespace FluentPOS.Shared.DTOs.Identity
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}