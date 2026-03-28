using LaundryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiagnosticsController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DiagnosticsController(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // TODO: Restaurar endpoint de stored procedures si es necesario
    // [HttpGet("stored-procedures")]
    // public async Task<IActionResult> GetStoredProcedures()
    // {
    //     var helper = new StoredProcedureHelper(_connectionFactory);
    //     var procedures = await helper.GetAllStoredProceduresAsync();
    //     return Ok(procedures);
    // }
}
