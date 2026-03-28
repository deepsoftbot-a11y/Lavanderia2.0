using LaundryManagement.Application.DTOs.Users;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.Users;

public record GetUsersQuery(
    string? Search,
    bool? IsActive,
    int? RoleId,
    string? SortBy,
    string? SortOrder
) : IRequest<List<UserResponseDto>>;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserResponseDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponseDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(
            search: request.Search,
            isActive: request.IsActive,
            roleId: request.RoleId,
            sortBy: request.SortBy,
            sortOrder: request.SortOrder,
            cancellationToken: cancellationToken);

        return users.Select(u =>
        {
            var assignment = u.RoleAssignments.FirstOrDefault();
            return new UserResponseDto
            {
                Id = u.Id.Value,
                Username = u.Username.Value,
                FullName = u.FullName,
                Email = u.Email.Value,
                IsActive = u.IsActive,
                Role = assignment != null
                    ? new UserRoleDto { Id = assignment.RoleId, Name = assignment.Role.Value }
                    : null,
                CreatedAt = u.CreatedAt.ToString("O"),
                LastLogin = u.LastLogin?.ToString("O"),
                CreatedBy = u.CreatedBy
            };
        }).ToList();
    }
}
