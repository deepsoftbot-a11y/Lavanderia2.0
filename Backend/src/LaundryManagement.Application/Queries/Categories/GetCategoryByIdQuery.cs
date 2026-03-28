using LaundryManagement.Application.DTOs.Categories;
using MediatR;

namespace LaundryManagement.Application.Queries.Categories;

public sealed record GetCategoryByIdQuery(int CategoryId) : IRequest<CategoryDto?>;
