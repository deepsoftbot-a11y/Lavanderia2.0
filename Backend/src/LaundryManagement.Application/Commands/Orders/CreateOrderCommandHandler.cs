using System.Text.Json;
using LaundryManagement.Application.Notifications;
using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.DomainEvents.Orders;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Orders;

/// <summary>
/// Handler que implementa el patrón DDD para crear órdenes.
/// Usa el agregado OrderPure, el repositorio de dominio y Unit of Work para transacciones atómicas.
/// </summary>
public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IPublisher _publisher;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ILogger<CreateOrderCommandHandler> logger,
        IPublisher publisher)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // 1. GENERAR FOLIO PRIMERO (consulta BD, fuera de transacción)
            var preGeneratedFolio = await _orderRepository.GenerateNextFolioAsync(cancellationToken);

            _logger.LogInformation("Folio generado: {Folio} para cliente {ClientId}",
                preGeneratedFolio, command.ClientId);

            // 2. Crear el agregado de dominio (sin folio todavía)
            var order = OrderPure.Create(
                clientId: ClientId.From(command.ClientId),
                promisedDate: command.PromisedDate,
                receivedBy: command.ReceivedBy,
                initialStatusId: command.InitialStatusId,
                notes: command.Notes,
                storageLocation: command.StorageLocation
            );

            // 3. Agregar líneas de items
            foreach (var item in command.Items)
            {
                order.AddLineItem(
                    serviceId: item.ServiceId,
                    serviceGarmentId: item.ServiceGarmentId,
                    weightKilos: item.WeightKilos,
                    quantity: item.Quantity,
                    unitPrice: Money.FromDecimal(item.UnitPrice),
                    lineDiscount: item.DiscountAmount.HasValue && item.DiscountAmount.Value > 0
                        ? Money.FromDecimal(item.DiscountAmount.Value)
                        : null,
                    notes: item.Notes
                );
            }

            // 4. Preparar datos del pago inicial si se proporciona
            InitialPaymentData? paymentData = null;
            if (command.InitialPayment != null)
            {
                // Validar que el monto no exceda el total de la orden
                if (command.InitialPayment.Amount > order.Total.Amount)
                {
                    throw new BusinessRuleException(
                        $"El monto del pago ({command.InitialPayment.Amount:C}) no puede exceder el total de la orden ({order.Total.Amount:C})"
                    );
                }

                paymentData = new InitialPaymentData(
                    Amount: command.InitialPayment.Amount,
                    ReceivedBy: command.ReceivedBy,
                    PaymentMethodsJson: CreatePaymentMethodsJson(command.InitialPayment),
                    Notes: command.InitialPayment.Notes
                );
            }

            // 5. Crear orden y pago atómicamente
            // El repositorio maneja la transacción internamente con ExecutionStrategy
            var (savedOrder, paymentId) = await _orderRepository.CreateOrderWithPaymentAsync(
                order,
                preGeneratedFolio,
                paymentData,
                cancellationToken
            );

            _logger.LogInformation(
                "Orden y pago creados exitosamente: OrdenID={OrderId}, Folio={Folio}, PagoID={PaymentId}",
                savedOrder.Id.Value, savedOrder.Folio.Value, paymentId
            );

            // 6. Publicar evento de dominio POST-COMMIT (best-effort)
            // El evento del agregado tiene OrderId=0 (pre-save); usamos el ID real del resultado.
            try
            {
                var domainEvent = new OrderCreated(
                    orderId: savedOrder.Id.Value,
                    clienteId: command.ClientId,
                    recibidoPor: command.ReceivedBy,
                    fechaRecepcion: DateTime.UtcNow);
                await _publisher.Publish(
                    new DomainEventNotification<OrderCreated>(domainEvent),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al publicar evento OrderCreated para OrdenID={OrderId}. El registro NO fue afectado.",
                    savedOrder.Id.Value);
            }

            // 7. Retornar resultado
            return new CreateOrderResult
            {
                OrderId = savedOrder.Id.Value,
                Folio = savedOrder.Folio.Value,
                Total = savedOrder.Total.Amount,
                PaymentId = paymentId,
                PaidAmount = command.InitialPayment?.Amount,
                RemainingBalance = command.InitialPayment != null
                    ? savedOrder.Total.Amount - command.InitialPayment.Amount
                    : null
            };
        }
        catch (BusinessRuleException)
        {
            // Re-throw business rule exceptions tal cual
            throw;
        }
        catch (ArgumentException ex)
        {
            // ArgumentException from value objects (e.g., ClientId, Money) are validation errors
            _logger.LogWarning(ex,
                "Validation error al crear orden. Cliente: {ClientId}",
                command.ClientId
            );

            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    { ex.ParamName ?? "Value", new[] { ex.Message } }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al crear orden y pago. Cliente: {ClientId}",
                command.ClientId
            );

            // Envolver otros errores en DatabaseException
            throw new DatabaseException(
                "Error al crear la orden y el pago.",
                ex
            );
        }
    }

    /// <summary>
    /// Crea el JSON de métodos de pago esperado por SP_RegistrarPago
    /// Formato: [{"MetodoPagoID": 2, "Monto": 300.00, "Referencia": ""}]
    /// </summary>
    private static string CreatePaymentMethodsJson(CreateOrderInitialPaymentDto payment)
    {
        var paymentMethods = new[]
        {
            new
            {
                MetodoPagoID = payment.PaymentMethodId,
                MontoPagado = payment.Amount,
                Referencia = payment.Reference ?? ""
            }
        };

        return JsonSerializer.Serialize(paymentMethods);
    }
}
