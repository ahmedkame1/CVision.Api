// DTOs/Auth/EmailConfirmationDto.cs
using System.ComponentModel.DataAnnotations;

namespace CVision.Api.DTOs.Auth
{
    public class EmailConfirmationRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class ResendConfirmationEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}