using LaundryManagement.Domain.Common;
using MediatR;

namespace LaundryManagement.Application.Notifications;

/// <summary>
/// Wrapper que permite publicar un DomainEvent (sin dependencia de MediatR) como INotification.
/// Se instancia en el command handler, después de que la transacción de BD confirma.
/// </summary>
public record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent)
    : INotification
    where TDomainEvent : DomainEvent;
