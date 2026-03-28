using LaundryManagement.Application.DTOs.Categories;
using MediatR;

namespace LaundryManagement.Application.Commands.Categories;

public sealed record UpdateCategoryCommand : IRequest<CategoryDto>
{
    public int CategoryId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
}
