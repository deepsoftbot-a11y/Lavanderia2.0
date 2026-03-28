using LaundryManagement.Application.DTOs.Categories;
using MediatR;

namespace LaundryManagement.Application.Commands.Categories;

public sealed record CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
