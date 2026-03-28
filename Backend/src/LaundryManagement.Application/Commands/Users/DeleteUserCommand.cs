using MediatR;

namespace LaundryManagement.Application.Commands.Users;

public record DeleteUserCommand(int Id) : IRequest;
