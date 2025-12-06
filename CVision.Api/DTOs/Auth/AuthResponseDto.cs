// DTOs/Auth/AuthResponseDto.cs
namespace CVision.Api.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public List<string> Roles { get; set; } = new List<string>();
        public UserType UserType { get; set; }
        public HrInfoDto? HrInfo { get; set; }
    }

    public class HrInfoDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        
    }
}