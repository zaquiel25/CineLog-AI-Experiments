using Microsoft.AspNetCore.Identity.UI.Services;

namespace Ezequiel_Movies.Services;

/// <summary>
/// FEATURE: Development-only email sender that logs email content to console.
/// Allows testing email flows locally by displaying confirmation URLs in the terminal.
/// </summary>
public class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogWarning("═══════════════════════════════════════════════════════");
        _logger.LogWarning("  DEV EMAIL — Not actually sent");
        _logger.LogWarning("  To:      {Email}", email);
        _logger.LogWarning("  Subject: {Subject}", subject);
        _logger.LogWarning("  Body:    {Body}", htmlMessage);
        _logger.LogWarning("═══════════════════════════════════════════════════════");
        return Task.CompletedTask;
    }
}
