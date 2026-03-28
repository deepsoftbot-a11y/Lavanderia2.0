using MediatR;

namespace LaundryManagement.Application.Commands.Roles;

public record DeleteRoleCommand(int Id) : IRequest;
