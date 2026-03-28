using LaundryManagement.Application.DTOs.Pagos;
using LaundryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    /// <summary>
    /// Registra un nuevo pago para una orden
    /// </summary>
    /// <param name="request">Datos del pago a registrar</param>
    /// <returns>ID del pago registrado</returns>
    /// <response code="200">Pago registrado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrarPagoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _pagoService.RegistrarPagoAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Consulta el saldo pendiente de un cliente
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Información del saldo del cliente</returns>
    /// <response code="200">Saldo consultado exitosamente</response>
    /// <response code="404">Cliente no encontrado</response>
    [HttpGet("saldo/{clienteId}")]
    [ProducesResponseType(typeof(ConsultarSaldoClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarSaldoCliente(int clienteId)
    {
        var result = await _pagoService.ConsultarSaldoClienteAsync(clienteId);
        return Ok(result);
    }
}
