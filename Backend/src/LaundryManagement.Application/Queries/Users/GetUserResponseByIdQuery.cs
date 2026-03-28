using LaundryManagement.Application.DTOs.Users;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Queries.Users;

public record GetUserResponseByIdQuery(int Id) : IRequest<UserResponseDto>;

public sealed class GetUserResponseByIdQueryHandler : IRequestHandler<GetUserResponseByIdQuery, UserResponseDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserResponseByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> Handle(GetUserResponseByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(UserId.From(request.Id), cancellationToken)
            ?? throw new NotFoundException($"Usuario con ID {request.Id} no encontrado");

        var assignment = user.RoleAssignments.FirstOrDefault();

        return new UserResponseDto
        {
            Id = user.Id.Value,
            Username = user.Username.Value,
            FullName = user.FullName,
            Email = user.Email.Value,
            IsActive = user.IsActive,
            Role = assignment != null
                ? new UserRoleDto { Id = assignment.RoleId, Name = assignment.Role.Value }
                : null,
            CreatedAt = user.CreatedAt.ToString("O"),
            LastLogin = user.LastLogin?.ToString("O"),
            CreatedBy = user.CreatedBy
        };
    }
}
