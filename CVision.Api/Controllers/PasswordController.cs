// Controllers/PasswordController.cs
using CVision.Api.DTOs.Password;
using CVision.Api.Models;
using CVision.Api.Services;
using CVision.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CVision.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PasswordController> _logger;

        public PasswordController(
            IPasswordService passwordService,
            UserManager<ApplicationUser> userManager,
            ILogger<PasswordController> logger)
        {
            _passwordService = passwordService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Change password result</returns>
        [HttpPost("change")]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), 200)]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ChangePasswordResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                // Basic model validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = errors
                    });
                }

                var userId = GetCurrentUserId();
                var result = await _passwordService.ChangePasswordAsync(userId, request);

                if (result.Success)
                {
                    _logger.LogInformation($"Password changed successfully for user ID: {userId}");
                    return Ok(result);
                }

                _logger.LogWarning($"Password change failed for user ID: {userId}");
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user: {GetCurrentUserId()}");
                return StatusCode(500, new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "An internal server error occurred",
                    Errors = new List<string> { "Please try again later" }
                });
            }
        }

        /// <summary>
        /// Validate password strength and requirements
        /// </summary>
        /// <param name="request">Password to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PasswordValidationResultDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PasswordValidationResultDto>> ValidatePassword([FromBody] PasswordValidationRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new PasswordValidationResultDto
                    {
                        IsValid = false,
                        Strength = "Empty",
                        Score = 0,
                        Requirements = new List<string> { "Password cannot be empty" },
                        Suggestions = new List<string> { "Please enter a password" }
                    });
                }

                var result = await _passwordService.ValidatePasswordAsync(request.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password");
                return StatusCode(500, new PasswordValidationResultDto
                {
                    IsValid = false,
                    Strength = "Error",
                    Score = 0,
                    Requirements = new List<string> { "Validation service error" },
                    Suggestions = new List<string> { "Please try again" }
                });
            }
        }

        /// <summary>
        /// Get password requirements
        /// </summary>
        /// <returns>Password requirements</returns>
        [HttpGet("requirements")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PasswordRequirementsDto), 200)]
        public async Task<ActionResult<PasswordRequirementsDto>> GetPasswordRequirements()
        {
            try
            {
                var requirements = await _passwordService.GetPasswordRequirementsAsync();
                return Ok(requirements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting password requirements");
                return StatusCode(500, new PasswordRequirementsDto
                {
                    Description = "Error retrieving password requirements"
                });
            }
        }

        /// <summary>
        /// Check if password is strong enough
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <returns>Strength check result</returns>
        [HttpPost("check-strength")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult> CheckPasswordStrength([FromQuery] string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return Ok(new
                    {
                        isStrong = false,
                        message = "Password is empty"
                    });
                }

                var isStrong = await _passwordService.IsPasswordStrongAsync(password);
                var validation = await _passwordService.ValidatePasswordAsync(password);

                return Ok(new
                {
                    isStrong,
                    strength = validation.Strength,
                    score = validation.Score,
                    isValid = validation.IsValid,
                    suggestions = validation.Suggestions,
                    requirements = validation.Requirements
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password strength");
                return StatusCode(500, new
                {
                    isStrong = false,
                    message = "Error checking password strength"
                });
            }
        }

        /// <summary>
        /// Get password security tips
        /// </summary>
        /// <returns>Security tips</returns>
        [HttpGet("security-tips")]
        [AllowAnonymous]
        public ActionResult GetSecurityTips()
        {
            var tips = new
            {
                generalTips = new[]
                {
                    " Use a minimum of 6 characters",
                    " Mix uppercase and lowercase letters",
                    " Include numbers in your password",
                    " Consider adding special characters for extra security",
                    " Avoid using personal information (name, birthdate, etc.)",
                    " Don't reuse passwords across different sites",
                    " Consider using a password manager",
                    " Change your password regularly"
                },
                examples = new[]
                {
                    "Good: Pass123!",
                    "Better: Secure@2024",
                    "Best: MyStrongP@ssw0rd!"
                },
                doNot = new[]
                {
                    " Don't use 'password' or '123456'",
                    " Don't use your name or username",
                    " Don't use common words or phrases",
                    " Don't write your password down where others can see it",
                    " Don't share your password with anyone"
                }
            };

            return Ok(tips);
        }

        /// <summary>
        /// Get current user's last password change date
        /// </summary>
        /// <returns>Last password change info</returns>
        [HttpGet("last-change")]
        [Authorize]
        public async Task<ActionResult> GetLastPasswordChange()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(new
                {
                    lastChange = user.ModifiedAt,
                    daysSinceChange = user.ModifiedAt.HasValue ?
                        (DateTime.Now - user.ModifiedAt.Value).Days : 0,
                    recommendation = user.ModifiedAt.HasValue &&
                        (DateTime.Now - user.ModifiedAt.Value).Days > 90 ?
                        "Consider changing your password for better security" :
                        "Your password is recently changed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last password change");
                return StatusCode(500, new { message = "Error retrieving password change info" });
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(_userManager.GetUserId(User)!);
        }
    }
}