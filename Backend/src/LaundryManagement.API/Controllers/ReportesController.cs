using LaundryManagement.Application.DTOs.Reportes;
using LaundryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    /// <summary>
    /// Genera el reporte de ventas diarias
    /// </summary>
    /// <param name="fecha">Fecha del reporte (formato: yyyy-MM-dd)</param>
    /// <returns>Resumen de ventas del día</returns>
    /// <response code="200">Reporte generado exitosamente</response>
    /// <response code="400">Fecha inválida</response>
    [HttpGet("ventas-diarias")]
    [ProducesResponseType(typeof(VentaDiariaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VentasDiarias([FromQuery] DateTime fecha)
    {
        var result = await _reporteService.ReporteVentasDiariasAsync(fecha);
        return Ok(result);
    }
}
