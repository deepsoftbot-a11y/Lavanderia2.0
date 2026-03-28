using MediatR;

namespace LaundryManagement.Application.Commands.Permissions;

public record DeletePermissionCommand(int Id) : IRequest;
