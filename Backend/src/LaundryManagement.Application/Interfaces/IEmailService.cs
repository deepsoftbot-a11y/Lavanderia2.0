namespace LaundryManagement.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(
        string[]          recipients,
        string            subject,
        string            body,
        string            fileName,
        byte[]            attachment,
        CancellationToken ct = default);
}
