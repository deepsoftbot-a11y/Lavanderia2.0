using LaundryManagement.Application.DTOs.Categories;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.Categories;

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(ICategoryRepository repository, ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting category by ID: {CategoryId}", query.CategoryId);

        var category = await _repository.GetByIdAsync(CategoryId.From(query.CategoryId), cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", query.CategoryId);
            return null;
        }

        return GetCategoriesQueryHandler.MapToDto(category);
    }
}
