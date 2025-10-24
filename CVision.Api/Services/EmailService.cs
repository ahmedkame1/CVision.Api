using CVision.Api.Helpers;
using CVision.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CVision.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirm Your Email - CVision";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Welcome to CVision!</h2>
                    <p>Thank you for registering with CVision. Please confirm your email address by clicking the button below:</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' 
                           style='background-color: #2563eb; color: white; padding: 12px 24px; 
                                  text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Confirm Email Address
                        </a>
                    </div>
                    
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #666;'>{confirmationLink}</p>
                    
                    <p>This link will expire in 24 hours.</p>
                    <br/>
                    <p>If you didn't create an account, please ignore this email.</p>
                    
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e5e5;'/>
                    <p style='color: #666; font-size: 12px;'>
                        Best regards,<br/>
                        The CVision Team
                    </p>
                </div>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "Reset Your Password - CVision";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #dc2626;'>Password Reset Request</h2>
                    <p>You requested to reset your password. Click the button below to set a new password:</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' 
                           style='background-color: #dc2626; color: white; padding: 12px 24px; 
                                  text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Reset Password
                        </a>
                    </div>
                    
                    <p>If you didn't request a password reset, please ignore this email.</p>
                    
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e5e5;'/>
                    <p style='color: #666; font-size: 12px;'>
                        Best regards,<br/>
                        The CVision Team
                    </p>
                </div>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Validate email address
                if (string.IsNullOrEmpty(toEmail))
                {
                    _logger.LogError("Email address is null or empty");
                    throw new ArgumentException("Email address cannot be empty");
                }

                _logger.LogInformation($"Attempting to send email to: {toEmail}");
                _logger.LogInformation($"SMTP Settings: {_emailSettings.SmtpServer}:{_emailSettings.Port}");

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = _emailSettings.Port,
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl,
                    Timeout = 30000
                };

                _logger.LogInformation("Sending email...");
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to: {toEmail}");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP Error sending to {toEmail}: {smtpEx.StatusCode} - {smtpEx.Message}");
                throw new InvalidOperationException($"Failed to send email: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"General Error sending to {toEmail}: {ex.Message}");
                throw new InvalidOperationException("Failed to send email. Please try again later.");
            }
        }
    }
}