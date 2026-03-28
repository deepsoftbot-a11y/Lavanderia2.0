using LaundryManagement.Application.DTOs.Categories;
using MediatR;

namespace LaundryManagement.Application.Commands.Categories;

public sealed record ToggleCategoryStatusCommand(int CategoryId) : IRequest<CategoryDto>;
