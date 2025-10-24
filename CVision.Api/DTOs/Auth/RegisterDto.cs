namespace CVision.Api.DTOs.Auth
{
    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? Website { get; set; }
        public string? ContactPerson { get; set; }
    }

    public enum UserType
    {
        User,
        HR
    }
}