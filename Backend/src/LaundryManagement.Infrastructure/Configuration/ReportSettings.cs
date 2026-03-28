namespace LaundryManagement.Infrastructure.Configuration;

public sealed class ReportSettings
{
    public const string SectionName = "ReportSettings";

    public bool   Enabled      { get; set; } = false;
    public string EmailFrom    { get; set; } = string.Empty;
    public string SmtpHost     { get; set; } = string.Empty;
    public int    SmtpPort     { get; set; } = 587;
    public string SmtpUser     { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool   SmtpUseSsl   { get; set; } = false;
}
