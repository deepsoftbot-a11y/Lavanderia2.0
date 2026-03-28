using LaundryManagement.Application.Commands.CashClosings;
using LaundryManagement.Application.DTOs.Cortes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Queries.CashClosings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/cortes-caja")]
[Authorize]
[Produces("application/json")]
public class CortesCajaController : ControllerBase
{
    private readonly ICorteCajaService _corteCajaService;
    private readonly IMediator _mediator;

    public CortesCajaController(ICorteCajaService corteCajaService, IMediator mediator)
    {
        _corteCajaService = corteCajaService;
        _mediator = mediator;
    }

    #region Nuevos Endpoints DDD (MediatR)

    /// <summary>
    /// Obtiene los totales de pagos del día para un cajero específico
    /// </summary>
    /// <param name="fecha">Fecha del día</param>
    /// <param name="cajeroId">ID del cajero</param>
    /// <returns>Totales por método de pago</returns>
    /// <response code="200">Totales obtenidos exitosamente</response>
    [HttpGet("totales-dia")]
    [ProducesResponseType(typeof(TotalesDiaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTotalesDia([FromQuery] DateTime fecha, [FromQuery] int cajeroId)
    {
        var result = await _mediator.Send(new GetTotalesDiaQuery(fecha, cajeroId));
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo corte de caja usando DDD
    /// </summary>
    /// <param name="command">Datos del corte de caja</param>
    /// <returns>ID del corte registrado</returns>
    /// <response code="200">Corte registrado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCashClosingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearCorteDDD([FromBody] CreateCashClosingCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    #endregion

    #region Endpoints Legacy (Servicio)

    /// <summary>
    /// Ajusta el monto de un corte de caja existente
    /// </summary>
    /// <param name="request">Datos del ajuste</param>
    /// <returns>Confirmación del ajuste</returns>
    /// <response code="200">Ajuste realizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Corte no encontrado</response>
    [HttpPut("ajustar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AjustarCorte([FromBody] AjustarCorteCajaRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _corteCajaService.AjustarCorteCajaAsync(request);
        return Ok(new { message = "Corte de caja ajustado exitosamente" });
    }

    /// <summary>
    /// Consulta el historial de cortes de caja
    /// </summary>
    /// <param name="cajeroId">ID del cajero (opcional)</param>
    /// <param name="fechaInicio">Fecha de inicio (opcional)</param>
    /// <param name="fechaFin">Fecha de fin (opcional)</param>
    /// <param name="soloConDiferencias">Mostrar solo cortes con diferencias</param>
    /// <returns>Lista de cortes</returns>
    /// <response code="200">Historial consultado exitosamente</response>
    [HttpGet("historial")]
    [ProducesResponseType(typeof(IEnumerable<HistorialCorteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultarHistorial(
        [FromQuery] int? cajeroId,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] bool soloConDiferencias = false)
    {
        var request = new ConsultarHistorialCortesRequest
        {
            CajeroID = cajeroId,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            SoloConDiferencias = soloConDiferencias
        };

        var result = await _corteCajaService.ConsultarHistorialCortesAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle completo de un corte de caja
    /// </summary>
    /// <param name="corteId">ID del corte</param>
    /// <returns>Detalle del corte</returns>
    /// <response code="200">Detalle obtenido exitosamente</response>
    /// <response code="404">Corte no encontrado</response>
    [HttpGet("{corteId}")]
    [ProducesResponseType(typeof(DetalleCorteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerDetalle(int corteId)
    {
        var result = await _corteCajaService.VerDetalleCorteAsync(corteId);
        return Ok(result);
    }

    #endregion
}
