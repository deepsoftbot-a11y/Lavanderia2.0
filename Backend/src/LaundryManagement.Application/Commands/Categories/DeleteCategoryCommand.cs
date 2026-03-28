using MediatR;

namespace LaundryManagement.Application.Commands.Categories;

public sealed record DeleteCategoryCommand(int CategoryId) : IRequest<Unit>;
