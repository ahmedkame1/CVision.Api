// DTOs/Password/PasswordDto.cs
using System.ComponentModel.DataAnnotations;

namespace CVision.Api.DTOs.Password
{
    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ChangedAt { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class PasswordValidationRequestDto
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    public class PasswordValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Strength { get; set; } = string.Empty;
        public int Score { get; set; } // 1-5
        public List<string> Requirements { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();
    }

    public class PasswordRequirementsDto
    {
        public int MinimumLength { get; set; } = 6;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public string Description { get; set; } = "Password must be at least 6 characters long and contain uppercase letters, lowercase letters, numbers, and symbols";
        public List<string> Examples { get; set; } = new List<string>
        {
            "Pass123!",
            "Secure@2024",
            "Admin#789"
        };
    }
}
