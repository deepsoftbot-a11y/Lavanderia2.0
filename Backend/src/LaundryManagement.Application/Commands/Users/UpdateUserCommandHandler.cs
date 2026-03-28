using LaundryManagement.Application.DTOs.Users;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Users;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(UserId.From(request.Id), cancellationToken)
            ?? throw new NotFoundException($"Usuario con ID {request.Id} no encontrado");

        if (request.FullName != null || request.Email != null)
        {
            var newFullName = request.FullName ?? user.FullName;
            var newEmail = user.Email;

            if (request.Email != null && request.Email != user.Email.Value)
            {
                if (await _userRepository.EmailExistsAsync(Email.From(request.Email), cancellationToken))
                    throw new BusinessRuleException($"Ya existe un usuario con el email '{request.Email}'");
                newEmail = Email.From(request.Email);
            }

            user.UpdateDetails(newFullName, newEmail);
        }

        if (request.Password != null)
        {
            var newHash = PasswordHash.FromHash(_passwordHasher.HashPassword(request.Password));
            user.ChangePassword(newHash, 0);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value && !user.IsActive)
                user.Activate(0);
            else if (!request.IsActive.Value && user.IsActive)
                user.Deactivate(0);
        }

        // Always process role (PUT semantics)
        string? roleName = null;
        int? resolvedRoleId = null;

        if (request.RoleId.HasValue)
        {
            var role = await _roleRepository.GetByIdAsync(RoleId.From(request.RoleId.Value), cancellationToken)
                ?? throw new NotFoundException($"Rol con ID {request.RoleId.Value} no encontrado");
            roleName = role.Name;
            resolvedRoleId = request.RoleId.Value;
            user.AssignRole(request.RoleId.Value, roleName);
        }
        else
        {
            user.RemoveAllRoles();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return CreateUserCommandHandler.MapToDto(user, resolvedRoleId, roleName);
    }
}
