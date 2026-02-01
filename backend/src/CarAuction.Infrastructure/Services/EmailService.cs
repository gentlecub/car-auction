using CarAuction.Application.Interfaces;
using CarAuction.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CarAuction.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit for SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string email, string token)
    {
        var verificationUrl = $"{_settings.BaseUrl}/verify-email?token={token}";

        var subject = "Verifica tu cuenta - CarAuction";
        var body = BuildEmailVerificationTemplate(verificationUrl);

        await SendEmailAsync(email, subject, body);
        _logger.LogInformation("Email verification sent to {Email}", email);
    }

    public async Task SendPasswordResetAsync(string email, string token)
    {
        var resetUrl = $"{_settings.BaseUrl}/reset-password?token={token}";

        var subject = "Restablecer contraseña - CarAuction";
        var body = BuildPasswordResetTemplate(resetUrl);

        await SendEmailAsync(email, subject, body);
        _logger.LogInformation("Password reset email sent to {Email}", email);
    }

    public async Task SendAuctionWonAsync(string email, string carName, decimal amount)
    {
        var subject = $"Felicitaciones! Ganaste la subasta - {carName}";
        var body = BuildAuctionWonTemplate(carName, amount);

        await SendEmailAsync(email, subject, body);
        _logger.LogInformation("Auction won notification sent to {Email} for {CarName}", email, carName);
    }

    public async Task SendOutbidNotificationAsync(string email, string carName, decimal newBid)
    {
        var subject = $"Te han superado en la subasta - {carName}";
        var body = BuildOutbidTemplate(carName, newBid);

        await SendEmailAsync(email, subject, body);
        _logger.LogInformation("Outbid notification sent to {Email} for {CarName}", email, carName);
    }

    /// <summary>
    /// Core method to send emails via SMTP
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (!_settings.EnableSending)
        {
            _logger.LogInformation(
                "[DEV MODE] Email would be sent to {Email}: {Subject}",
                toEmail, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = StripHtml(htmlBody)
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            var secureOption = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secureOption);

            if (!string.IsNullOrEmpty(_settings.SmtpUser))
            {
                await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogDebug("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            throw;
        }
    }

    /// <summary>
    /// Strip HTML tags for plain text version
    /// </summary>
    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ")
            .Replace("&nbsp;", " ")
            .Replace("  ", " ")
            .Trim();
    }

    #region Email Templates

    private string BuildEmailVerificationTemplate(string verificationUrl)
    {
        return BuildBaseTemplate(
            "Verifica tu cuenta",
            $@"
            <p>Gracias por registrarte en CarAuction.</p>
            <p>Por favor, verifica tu cuenta haciendo clic en el siguiente enlace:</p>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='{verificationUrl}' class='button'>Verificar mi cuenta</a>
            </p>
            <p style='color: #666; font-size: 14px;'>
                Si no creaste esta cuenta, puedes ignorar este email.
            </p>
            <p style='color: #666; font-size: 12px;'>
                Este enlace expirara en 24 horas.
            </p>"
        );
    }

    private string BuildPasswordResetTemplate(string resetUrl)
    {
        return BuildBaseTemplate(
            "Restablecer contraseña",
            $@"
            <p>Recibimos una solicitud para restablecer tu contraseña.</p>
            <p>Haz clic en el siguiente enlace para crear una nueva contraseña:</p>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='{resetUrl}' class='button'>Restablecer contraseña</a>
            </p>
            <p style='color: #666; font-size: 14px;'>
                Si no solicitaste este cambio, ignora este email.
                Tu contraseña permanecera sin cambios.
            </p>
            <p style='color: #666; font-size: 12px;'>
                Este enlace expirara en 1 hora.
            </p>"
        );
    }

    private string BuildAuctionWonTemplate(string carName, decimal amount)
    {
        return BuildBaseTemplate(
            "Felicitaciones! Ganaste la subasta",
            $@"
            <p style='font-size: 18px; color: #22C55E;'>
                <strong>Has ganado la subasta!</strong>
            </p>
            <div style='background: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                <p><strong>Vehiculo:</strong> {carName}</p>
                <p><strong>Monto ganador:</strong> ${amount:N2}</p>
            </div>
            <p>Nos pondremos en contacto contigo pronto con los detalles para completar la transaccion.</p>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='{_settings.BaseUrl}/my-auctions' class='button'>Ver mis subastas</a>
            </p>"
        );
    }

    private string BuildOutbidTemplate(string carName, decimal newBid)
    {
        return BuildBaseTemplate(
            "Te han superado en la subasta",
            $@"
            <p style='font-size: 18px; color: #EF4444;'>
                <strong>Alguien ha superado tu oferta!</strong>
            </p>
            <div style='background: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                <p><strong>Vehiculo:</strong> {carName}</p>
                <p><strong>Nueva oferta mas alta:</strong> ${newBid:N2}</p>
            </div>
            <p>No pierdas esta oportunidad! Haz una nueva oferta ahora.</p>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='{_settings.BaseUrl}' class='button'>Hacer nueva oferta</a>
            </p>"
        );
    }

    private string BuildBaseTemplate(string title, string content)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            color: #111827;
            margin: 0;
            padding: 0;
            background-color: #f3f4f6;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .email-body {{
            background: #ffffff;
            border-radius: 8px;
            padding: 40px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 28px;
            font-weight: bold;
            color: #1E40AF;
        }}
        h1 {{
            color: #111827;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        .button {{
            display: inline-block;
            background: #1E40AF;
            color: #ffffff !important;
            padding: 14px 28px;
            text-decoration: none;
            border-radius: 6px;
            font-weight: 600;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e5e7eb;
            color: #6b7280;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='email-body'>
            <div class='header'>
                <div class='logo'>CarAuction</div>
            </div>
            <h1>{title}</h1>
            {content}
            <div class='footer'>
                <p>&copy; {DateTime.UtcNow.Year} CarAuction. Todos los derechos reservados.</p>
                <p>Este es un email automatico, por favor no respondas a este mensaje.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    #endregion
}
