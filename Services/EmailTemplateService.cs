namespace Ezequiel_Movies.Services;

/// <summary>
/// FEATURE: Generates branded HTML email templates for FrameRoute.
/// All emails share a consistent dark theme with Cinema Gold branding.
/// Uses inline CSS for maximum email client compatibility.
/// </summary>
public static class EmailTemplateService
{
    private const string LogoUrl = "https://frameroute.net/images/logo-gold.png";
    private const string Gold = "#f4d03f";
    private const string DarkBg = "#1a1a2e";
    private const string CardBg = "#222244";
    private const string TextColor = "#e0e0e0";
    private const string MutedText = "#999999";

    /// <summary>
    /// FEATURE: Wraps email content in the FrameRoute branded template.
    /// </summary>
    public static string Wrap(string title, string bodyHtml)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin:0; padding:0; background-color:{DarkBg}; font-family:Arial, Helvetica, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:{DarkBg}; padding:40px 20px;"">
        <tr>
            <td align=""center"">
                <table width=""500"" cellpadding=""0"" cellspacing=""0"" style=""max-width:500px; width:100%;"">
                    <!-- Logo -->
                    <tr>
                        <td align=""center"" style=""padding-bottom:30px;"">
                            <img src=""{LogoUrl}"" alt=""FrameRoute"" height=""50"" style=""height:50px;"">
                        </td>
                    </tr>
                    <!-- Card -->
                    <tr>
                        <td style=""background-color:{CardBg}; border-radius:12px; padding:40px 30px; border:1px solid #333366;"">
                            <h2 style=""color:{Gold}; margin:0 0 20px 0; font-size:22px; text-align:center;"">{title}</h2>
                            <div style=""color:{TextColor}; font-size:15px; line-height:1.6;"">
                                {bodyHtml}
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""padding-top:25px; text-align:center;"">
                            <p style=""color:{MutedText}; font-size:12px; margin:0;"">
                                FrameRoute — Your personal movie tracking companion
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    /// <summary>
    /// FEATURE: Generates a styled CTA button for emails.
    /// </summary>
    public static string Button(string text, string url)
    {
        return $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:25px 0;"">
            <tr>
                <td align=""center"">
                    <a href=""{url}"" style=""display:inline-block; background-color:{Gold}; color:#1a1a2e; text-decoration:none; padding:14px 32px; border-radius:8px; font-weight:bold; font-size:15px;"">{text}</a>
                </td>
            </tr>
        </table>";
    }

    /// <summary>
    /// FEATURE: Email confirmation email for new registrations.
    /// Includes brief app description to welcome new users.
    /// </summary>
    public static string ConfirmEmail(string callbackUrl)
    {
        var body = $@"
            <p>Keep track of every movie you watch — rate them, get personalized suggestions, and build your own movie journal.</p>
            <p>To get started, confirm your email:</p>
            {Button("Confirm Email", callbackUrl)}
            <p style=""color:{MutedText}; font-size:13px; text-align:center;"">If you didn't create this account, just ignore this email.</p>";

        return Wrap("Welcome to FrameRoute", body);
    }

    /// <summary>
    /// FEATURE: Password reset email.
    /// </summary>
    public static string ResetPassword(string callbackUrl)
    {
        var body = $@"
            <p>We received a request to reset your password.</p>
            <p>Click the button below to choose a new one:</p>
            {Button("Reset Password", callbackUrl)}
            <p style=""color:{MutedText}; font-size:13px; text-align:center;"">If you didn't request this, just ignore this email. Your password won't change.</p>";

        return Wrap("Reset Your Password", body);
    }
}
