namespace CVision.Api.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public UserType UserType { get; set; }
        public HrInfoDto? HrInfo { get; set; }
    }

    public class HrInfoDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
    }
}