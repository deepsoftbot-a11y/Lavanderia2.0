using LaundryManagement.Application.DTOs.Users;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;

namespace LaundryManagement.Application.Commands.Users;

public sealed class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, UserResponseDto>
{
    private readonly IUserRepository _userRepository;

    public ToggleUserStatusCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(UserId.From(request.Id), cancellationToken)
            ?? throw new NotFoundException($"Usuario con ID {request.Id} no encontrado");

        if (user.IsActive)
            user.Deactivate(0);
        else
            user.Activate(0);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return CreateUserCommandHandler.MapToDto(user);
    }
}
