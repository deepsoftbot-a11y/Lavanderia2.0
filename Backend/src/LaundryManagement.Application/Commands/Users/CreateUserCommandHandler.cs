using LaundryManagement.Application.DTOs.Users;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Aggregates.Users;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Users;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var username = Username.From(request.Username);
        var email = Email.From(request.Email);

        if (await _userRepository.ExistsAsync(username, cancellationToken))
            throw new BusinessRuleException($"Ya existe un usuario con el nombre '{request.Username}'");

        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
            throw new BusinessRuleException($"Ya existe un usuario con el email '{request.Email}'");

        var passwordHash = PasswordHash.FromHash(_passwordHasher.HashPassword(request.Password));
        var user = UserPure.Create(username, email, passwordHash, request.FullName);

        if (!request.IsActive)
            user.Deactivate(0);

        string? roleName = null;
        if (request.RoleId.HasValue)
        {
            var role = await _roleRepository.GetByIdAsync(RoleId.From(request.RoleId.Value), cancellationToken)
                ?? throw new NotFoundException($"Rol con ID {request.RoleId.Value} no encontrado");
            roleName = role.Name;
            user.AssignRole(request.RoleId.Value, roleName);
        }

        await _userRepository.AddAsync(user, cancellationToken);

        return MapToDto(user, request.RoleId, roleName);
    }

    internal static UserResponseDto MapToDto(UserPure user, int? roleId = null, string? roleName = null)
    {
        var assignment = user.RoleAssignments.FirstOrDefault();
        var resolvedRoleId = roleId ?? assignment?.RoleId;
        var resolvedRoleName = roleName ?? assignment?.Role.Value;

        return new UserResponseDto
        {
            Id = user.Id.Value,
            Username = user.Username.Value,
            FullName = user.FullName,
            Email = user.Email.Value,
            IsActive = user.IsActive,
            Role = resolvedRoleId.HasValue && resolvedRoleName != null
                ? new UserRoleDto { Id = resolvedRoleId.Value, Name = resolvedRoleName }
                : null,
            CreatedAt = user.CreatedAt.ToString("O"),
            LastLogin = user.LastLogin?.ToString("O"),
            CreatedBy = user.CreatedBy
        };
    }
}
