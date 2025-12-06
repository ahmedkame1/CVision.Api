// Services/AuthService.cs
using CVision.Api.Data;
using CVision.Api.DTOs.Auth;
using CVision.Api.Models;
using CVision.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CVision.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TokenService _tokenService;
        private readonly CVisionDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            TokenService tokenService,
            CVisionDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation($" LOGIN ATTEMPT - Email: {loginDto.Email}");

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning($" LOGIN FAILED - User not found: {loginDto.Email}");
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            _logger.LogInformation($" USER FOUND - ID: {user.Id}, Email: {user.Email}, EmailConfirmed: {user.EmailConfirmed}, IsDeleted: {user.IsDeleted}");

            // Check email confirmation
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning($" LOGIN FAILED - Email not confirmed: {user.Email}");
                throw new UnauthorizedAccessException("Please confirm your email address before logging in");
            }

            // استخدام CheckPassword مباشرة
            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning($" LOGIN FAILED - Invalid password for: {loginDto.Email}");
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning($" LOGIN FAILED - Account deactivated: {loginDto.Email}");
                throw new UnauthorizedAccessException("Account is deactivated");
            }

            _logger.LogInformation($" LOGIN SUCCESSFUL - Generating token for: {user.Email}");

            // توليد التوكن مباشرة
            var token = await _tokenService.GenerateToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            HrInfoDto? hrInfo = null;
            if (roles.Contains("HR"))
            {
                var hrProfile = await _context.Hrs.FirstOrDefaultAsync(h => h.UserId == user.Id);
                if (hrProfile != null)
                {
                    hrInfo = new HrInfoDto
                    {
                        CompanyName = hrProfile.CompanyName,
                        CompanyAddress = hrProfile.CompanyAddress,
                        
                    };
                }
            }

            var userType = roles.Contains("HR") ? UserType.HR : UserType.User;

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(60),
                UserId = user.Id.ToString(),
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                UserType = userType,
                HrInfo = hrInfo
            };
        }

        // في RegisterAsync method
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation($"Registration attempt for email: {registerDto.Email}");

            if (registerDto.Password != registerDto.ConfirmPassword)
                throw new ArgumentException("Passwords don't match");

            if (registerDto.UserType == UserType.HR)
            {
                if (string.IsNullOrEmpty(registerDto.CompanyName))
                    throw new ArgumentException("Company name is required for HR registration");

                
            }

            // إنشاء المستخدم مع FirstName و LastName
            var user = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                CreatedAt = DateTime.Now,
                EmailConfirmed = false
            };

            _logger.LogInformation($"Creating user with email: {registerDto.Email}");

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"User creation failed: {errors}");
                throw new ArgumentException($"User creation failed: {errors}");
            }

            _logger.LogInformation($"User created successfully: ID={user.Id}, Email={user.Email}");

            // Generate email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            user.EmailConfirmationToken = emailToken;
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                _logger.LogError($"Failed to update user with email token: {errors}");
                throw new ArgumentException("Failed to generate confirmation token");
            }

            _logger.LogInformation($"Email token generated for user: {user.Email}");

            string roleName = registerDto.UserType == UserType.HR ? "HR" : "User";
            var roleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError($"Failed to assign role: {errors}");
            }
            else
            {
                _logger.LogInformation($"Role {roleName} assigned to user: {user.Email}");
            }

            if (registerDto.UserType == UserType.HR)
            {
                var hrProfile = new Hr
                {
                    UserId = user.Id,
                    CompanyName = registerDto.CompanyName!,
                    CompanyAddress = registerDto.CompanyAddress ?? "",
                   
                };

                _context.Hrs.Add(hrProfile);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"HR profile created for user: {user.Email}");
            }

            // Send confirmation email
            try
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7031";
                var confirmationLink = $"{baseUrl}/api/auth/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(emailToken)}";

                _logger.LogInformation($"Sending confirmation email to: {user.Email}");
                _logger.LogInformation($"Confirmation link: {confirmationLink}");

                await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);
                _logger.LogInformation($"Confirmation email sent successfully to: {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send confirmation email to: {user.Email}");
                // Don't throw here - registration should succeed even if email fails
            }

            var token = await _tokenService.GenerateToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            HrInfoDto? hrInfo = null;
            if (registerDto.UserType == UserType.HR)
            {
                hrInfo = new HrInfoDto
                {
                    CompanyName = registerDto.CompanyName!,
                    CompanyAddress = registerDto.CompanyAddress ?? "",
                    
                };
            }

            _logger.LogInformation($"Registration completed successfully for: {user.Email}");

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(60),
                UserId = user.Id.ToString(),
                Email = user.Email!,
                FirstName = user.FirstName, 
                LastName = user.LastName,
                Roles = roles.ToList(),
                UserType = registerDto.UserType,
                HrInfo = hrInfo
            };
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            _logger.LogInformation($"Email confirmation attempt for: {email}");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"Email confirmation failed - User not found: {email}");
                return false;
            }

            if (user.EmailConfirmationToken != token || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning($"Email confirmation failed - Invalid or expired token for: {email}");
                return false;
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Email confirmed successfully for: {email}");
            }
            else
            {
                _logger.LogError($"Failed to update user after email confirmation: {email}");
            }

            return result.Succeeded;
        }

        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            _logger.LogInformation($"Resend confirmation email attempt for: {email}");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"Resend confirmation failed - User not found: {email}");
                return false;
            }

            if (user.EmailConfirmed)
            {
                _logger.LogWarning($"Resend confirmation failed - Email already confirmed: {email}");
                return false;
            }

            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            user.EmailConfirmationToken = emailToken;
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError($"Failed to update user token for resend: {email}");
                return false;
            }

            try
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7009";
                var confirmationLink = $"{baseUrl}/api/auth/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(emailToken)}";

                await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);
                _logger.LogInformation($"Confirmation email resent successfully to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to resend confirmation email to: {email}");
                return false;
            }
        }

        public async Task<bool> AssignRoleAsync(string email, string roleName)
        {
            _logger.LogInformation($"Assign role attempt: {roleName} to {email}");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"Assign role failed - User not found: {email}");
                return false;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded && roleName == "HR")
            {
                var existingHr = await _context.Hrs.FirstOrDefaultAsync(h => h.UserId == user.Id);
                if (existingHr == null)
                {
                    var hrProfile = new Hr
                    {
                        UserId = user.Id,
                        CompanyName = "Default Company",
                        CompanyAddress = "",
                        
                    };

                    _context.Hrs.Add(hrProfile);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"HR profile created during role assignment for: {email}");
                }
            }

            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} assigned successfully to: {email}");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Failed to assign role {roleName} to {email}: {errors}");
            }

            return result.Succeeded;
        }
    }
}