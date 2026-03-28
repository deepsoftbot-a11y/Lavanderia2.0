using LaundryManagement.Application.Notifications;
using LaundryManagement.Domain.Aggregates.CashClosings;
using LaundryManagement.Domain.DomainEvents.CashClosings;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.CashClosings;

/// <summary>
/// Handler para crear un nuevo corte de caja
/// Usa el agregado de dominio CashClosingPure para la lógica de negocio
/// </summary>
public sealed class CreateCashClosingCommandHandler : IRequestHandler<CreateCashClosingCommand, CreateCashClosingResponse>
{
    private readonly ICashClosingRepository _repository;
    private readonly ILogger<CreateCashClosingCommandHandler> _logger;
    private readonly IPublisher _publisher;

    public CreateCashClosingCommandHandler(
        ICashClosingRepository repository,
        ILogger<CreateCashClosingCommandHandler> logger,
        IPublisher publisher)
    {
        _repository  = repository  ?? throw new ArgumentNullException(nameof(repository));
        _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
        _publisher   = publisher   ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<CreateCashClosingResponse> Handle(CreateCashClosingCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("Creando corte de caja para cajero {CajeroId}", cmd.CajeroID);

        // Crear agregado de dominio con lógica de negocio
        var cashClosing = CashClosingPure.Create(
            cashierId: cmd.CajeroID,
            startDate: cmd.FechaInicio,
            endDate: cmd.FechaFin,
            expectedCash: Money.FromDecimal(cmd.TotalEsperadoEfectivo),
            expectedCard: Money.FromDecimal(cmd.TotalEsperadoTarjeta),
            expectedTransfer: Money.FromDecimal(cmd.TotalEsperadoTransferencia),
            expectedOther: Money.FromDecimal(cmd.TotalEsperadoOtros),
            declaredCash: Money.FromDecimal(cmd.TotalDeclaradoEfectivo),
            declaredCard: Money.FromDecimal(cmd.TotalDeclaradoTarjeta),
            declaredTransfer: Money.FromDecimal(cmd.TotalDeclaradoTransferencia),
            declaredOther: Money.FromDecimal(cmd.TotalDeclaradoOtros),
            adjustmentAmount: Money.FromDecimal(cmd.MontoAjuste),
            adjustmentReason: cmd.MotivoAjuste,
            shiftDescription: cmd.TurnoDescripcion,
            notes: cmd.Observaciones,
            initialFund: cmd.FondoInicial.HasValue ? Money.FromDecimal(cmd.FondoInicial.Value) : null
        );

        // Persistir vía repositorio (genera folio automáticamente)
        var saved = await _repository.AddAsync(cashClosing, ct);

        _logger.LogInformation(
            "Corte de caja creado exitosamente. ID: {CorteId}, Folio: {Folio}",
            saved.Id.Value,
            saved.Folio?.Value ?? "Sin asignar");

        // Publicar evento post-commit (fire-and-forget)
        try
        {
            var domainEvent = new CashClosingCreated(
                cashClosingId:   saved.Id.Value,
                cashierId:       cmd.CajeroID,
                closingDate:     cmd.FechaCorte,
                totalExpected:   cmd.TotalEsperado,
                totalDeclared:   cmd.TotalDeclarado,
                finalDifference: cmd.TotalDeclarado - cmd.TotalEsperado);

            await _publisher.Publish(
                new DomainEventNotification<CashClosingCreated>(domainEvent), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al publicar evento CashClosingCreated para CorteID={CorteId}. El registro NO fue afectado.",
                saved.Id.Value);
        }

        return new CreateCashClosingResponse { CorteID = saved.Id.Value };
    }
}
