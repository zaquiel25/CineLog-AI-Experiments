using Microsoft.AspNetCore.Identity.UI.Services;
using Resend;

namespace Ezequiel_Movies.Services;

/// <summary>
/// FEATURE: Email sender implementation using Resend API.
/// Wraps the IResend client to implement ASP.NET Identity's IEmailSender interface.
/// Configured via Resend:ApiKey (user-secrets/Key Vault) and Resend:FromEmail settings.
/// </summary>
public class ResendEmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public ResendEmailSender(
        IResend resend,
        ILogger<ResendEmailSender> logger,
        IConfiguration configuration)
    {
        _resend = resend;
        _logger = logger;
        _fromEmail = configuration["Resend:FromEmail"] ?? "noreply@frameroute.net";
        _fromName = configuration["Resend:FromName"] ?? "FrameRoute";
    }

    /// <summary>
    /// FEATURE: Sends an email via Resend API with error handling and logging.
    /// </summary>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var message = new EmailMessage();
            message.From = $"{_fromName} <{_fromEmail}>";
            message.To.Add(email);
            message.Subject = subject;
            message.HtmlBody = htmlMessage;

            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Email sent to {Email} with subject '{Subject}'", email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject '{Subject}'", email, subject);
            throw;
        }
    }
}
