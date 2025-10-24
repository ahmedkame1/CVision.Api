using System.ComponentModel.DataAnnotations;

namespace CVision.Api.DTOs.Auth
{
    public class AssignRoleDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}