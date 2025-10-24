// Services/Interfaces/IEmailService.cs
namespace CVision.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendPasswordResetAsync(string email, string resetLink);
    }
}