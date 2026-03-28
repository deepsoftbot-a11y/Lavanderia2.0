using LaundryManagement.Application.DTOs.Auth;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Auth;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(UserId.From(request.UserId), cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id.Value,
            Username = user.Username.Value,
            Name = user.FullName,
            Email = user.Email.Value,
            Role = user.PrimaryRole?.Value ?? "empleado",
            Status = user.IsActive ? "active" : "inactive",
            Permissions = user.Permissions.Select(p => p.PermissionName).ToList(),
            CreatedAt = user.CreatedAt.ToString("O"), // ISO8601
            CreatedBy = user.CreatedBy,
            LastLogin = user.LastLogin?.ToString("O") // ISO8601
        };
    }
}
