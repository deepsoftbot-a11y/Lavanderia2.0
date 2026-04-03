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

    /// <summary>
    /// Exporta el historial de órdenes con filtros a Excel o PDF
    /// </summary>
    [HttpGet("ordenes/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportOrdenes(
        [FromQuery] string format,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int[]? statusIds,
        [FromQuery] string[]? paymentStatuses)
    {
        if (format != "xlsx" && format != "pdf")
            return BadRequest(new { message = "format debe ser 'xlsx' o 'pdf'" });

        if (format == "xlsx")
        {
            var bytes = await _reporteService.ExportOrdenesExcelAsync(startDate, endDate, statusIds, paymentStatuses);
            var fileName = $"ordenes-{DateTime.Today:yyyy-MM-dd}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        else
        {
            var bytes = await _reporteService.ExportOrdenesPdfAsync(startDate, endDate, statusIds, paymentStatuses);
            var fileName = $"ordenes-{DateTime.Today:yyyy-MM-dd}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}
