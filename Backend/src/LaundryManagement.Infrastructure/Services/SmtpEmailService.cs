using LaundryManagement.Application.Interfaces;
using LaundryManagement.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LaundryManagement.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly ReportSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<ReportSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    public async Task SendAsync(
        string[]          recipients,
        string            subject,
        string            body,
        string            fileName,
        byte[]            attachment,
        CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.EmailFrom));

        foreach (var r in recipients)
            message.To.Add(MailboxAddress.Parse(r.Trim()));

        message.Subject = subject;

        var builder = new BodyBuilder { TextBody = body };
        builder.Attachments.Add(fileName, attachment);
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        _logger.LogInformation("Enviando reporte {FileName} a {Count} destinatarios", fileName, recipients.Length);

        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, ct);

        if (!string.IsNullOrEmpty(_settings.SmtpUser))
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email enviado exitosamente: {Subject}", subject);
    }
}
