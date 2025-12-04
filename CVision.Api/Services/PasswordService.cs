// Services/PasswordService.cs
using CVision.Api.Data;
using CVision.Api.DTOs.Password;
using CVision.Api.Models;
using CVision.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CVision.Api.Services
{
    public interface IPasswordService
    {
        Task<ChangePasswordResponseDto> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
        Task<PasswordValidationResultDto> ValidatePasswordAsync(string password);
        Task<PasswordRequirementsDto> GetPasswordRequirementsAsync();
        Task<bool> IsPasswordStrongAsync(string password);
    }

    public class PasswordService : IPasswordService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(
            UserManager<ApplicationUser> userManager,
            ILogger<PasswordService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ChangePasswordResponseDto> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            _logger.LogInformation($"Password change attempt for user ID: {userId}");

            var response = new ChangePasswordResponseDto();
            var errors = new List<string>();

            try
            {
                // Find user
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {userId}");
                    errors.Add("User not found");
                    response.Success = false;
                    response.Message = "User not found";
                    response.Errors = errors;
                    return response;
                }

                // Validate current password
                if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
                {
                    _logger.LogWarning($"Invalid current password for user: {user.Email}");
                    errors.Add("Current password is incorrect");
                    response.Success = false;
                    response.Message = "Current password is incorrect";
                    response.Errors = errors;
                    return response;
                }

                // Validate new password meets requirements
                var validationResult = await ValidatePasswordAsync(request.NewPassword);
                if (!validationResult.IsValid)
                {
                    errors.AddRange(validationResult.Suggestions);
                    response.Success = false;
                    response.Message = "New password does not meet requirements";
                    response.Errors = errors;
                    return response;
                }

                // Check if new password is same as current
                if (request.NewPassword == request.CurrentPassword)
                {
                    errors.Add("New password must be different from current password");
                    response.Success = false;
                    response.Message = "New password must be different from current password";
                    response.Errors = errors;
                    return response;
                }

                // Check password confirmation
                if (request.NewPassword != request.ConfirmPassword)
                {
                    errors.Add("New password and confirmation do not match");
                    response.Success = false;
                    response.Message = "New password and confirmation do not match";
                    response.Errors = errors;
                    return response;
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (result.Succeeded)
                {
                    // Update user modification timestamp
                    user.ModifiedAt = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation($"Password changed successfully for user: {user.Email}");

                    response.Success = true;
                    response.Message = "Password changed successfully";
                    response.ChangedAt = DateTime.Now;

                    // Send email notification (optional)
                    // await SendPasswordChangeNotificationAsync(user.Email);
                }
                else
                {
                    errors.AddRange(result.Errors.Select(e => e.Description));
                    _logger.LogError($"Failed to change password: {string.Join(", ", errors)}");

                    response.Success = false;
                    response.Message = "Failed to change password";
                    response.Errors = errors;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user: {userId}");
                errors.Add("An error occurred while changing password");
                response.Success = false;
                response.Message = "An error occurred while changing password";
                response.Errors = errors;
            }

            return response;
        }

        public async Task<PasswordValidationResultDto> ValidatePasswordAsync(string password)
        {
            var result = new PasswordValidationResultDto();
            var requirements = new List<string>();
            var suggestions = new List<string>();
            int score = 0;

            // Check minimum length (6 characters)
            if (password.Length >= 6)
            {
                score++;
                if (password.Length >= 8) score++; // Bonus for longer password
            }
            else
            {
                requirements.Add("Must be at least 6 characters long");
                suggestions.Add("Add more characters (minimum 6)");
            }

            // Check for uppercase letters
            if (password.Any(char.IsUpper))
            {
                score++;
            }
            else
            {
                requirements.Add("Must contain at least one uppercase letter (A-Z)");
                suggestions.Add("Add at least one uppercase letter");
            }

            // Check for lowercase letters
            if (password.Any(char.IsLower))
            {
                score++;
            }
            else
            {
                requirements.Add("Must contain at least one lowercase letter (a-z)");
                suggestions.Add("Add at least one lowercase letter");
            }

            // Check for digits
            if (password.Any(char.IsDigit))
            {
                score++;
            }
            else
            {
                requirements.Add("Must contain at least one number (0-9)");
                suggestions.Add("Add at least one number");
            }

            // Check for special characters (bonus, not required in basic setup)
            if (password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                score++;
            }
            else
            {
                suggestions.Add("Consider adding a special character (!@#$%^&*) for better security");
            }

            // Determine password strength
            result.Score = score;
            result.Requirements = requirements;
            result.Suggestions = suggestions;
            result.IsValid = requirements.Count == 0;

            result.Strength = score switch
            {
                >= 5 => "Very Strong",
                4 => "Strong",
                3 => "Good",
                2 => "Fair",
                1 => "Weak",
                _ => "Very Weak"
            };

            return await Task.FromResult(result);
        }

        public async Task<PasswordRequirementsDto> GetPasswordRequirementsAsync()
        {
            return await Task.FromResult(new PasswordRequirementsDto());
        }

        public async Task<bool> IsPasswordStrongAsync(string password)
        {
            var result = await ValidatePasswordAsync(password);
            return result.IsValid && result.Score >= 3; // At least "Good" strength
        }

        private async Task SendPasswordChangeNotificationAsync(string email)
        {
            try
            {
                // Implement email sending logic here
                _logger.LogInformation($"Password change notification would be sent to: {email}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password change notification");
            }
        }
    }
}