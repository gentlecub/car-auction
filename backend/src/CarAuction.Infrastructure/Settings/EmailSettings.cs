namespace CarAuction.Infrastructure.Settings;

/// <summary>
/// Configuration settings for email service (SMTP)
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// SMTP server hostname
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (typically 587 for TLS, 465 for SSL)
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username for authentication
    /// </summary>
    public string SmtpUser { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password for authentication
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address (From field)
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Sender display name
    /// </summary>
    public string FromName { get; set; } = "CarAuction";

    /// <summary>
    /// Use SSL/TLS for SMTP connection
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Enable email sending (false = only log, useful for development)
    /// </summary>
    public bool EnableSending { get; set; } = true;

    /// <summary>
    /// Frontend base URL for email links
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5173";
}
