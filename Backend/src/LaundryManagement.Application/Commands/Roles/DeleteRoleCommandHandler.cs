using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Roles;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;

    public DeleteRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(RoleId.From(request.Id), cancellationToken)
            ?? throw new NotFoundException($"Rol con ID {request.Id} no encontrado");

        if (role.IsActive)
            role.Deactivate();

        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);
    }
}
