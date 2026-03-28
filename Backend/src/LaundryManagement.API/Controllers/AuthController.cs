using LaundryManagement.Application.Commands.Auth;
using LaundryManagement.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

/// <summary>
/// Controller for authentication endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Validates a JWT token and returns user information
    /// </summary>
    [HttpGet("validate")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateToken()
    {
        // Extract token from Authorization header
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Ok(new
            {
                success = false,
                message = "Token no proporcionado"
            });
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var query = new ValidateTokenQuery(token);
        var response = await _mediator.Send(query);

        return Ok(response);
    }

    /// <summary>
    /// Logs out the current user (JWT is stateless, so this just confirms the action)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        // JWT is stateless, so logout is handled client-side by removing the token
        return Ok(new
        {
            success = true,
            message = "Logout exitoso"
        });
    }
}
