using LaundryManagement.Application.DTOs.Categories;
using LaundryManagement.Domain.Aggregates.Categories;
using LaundryManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Queries.Categories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(ICategoryRepository repository, ILogger<GetCategoriesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all categories");
        var categories = await _repository.GetAllAsync(cancellationToken);
        return categories.Select(MapToDto).ToList();
    }

    internal static CategoryDto MapToDto(CategoryPure category) => new()
    {
        Id = category.Id.Value,
        Name = category.Name,
        Description = category.Description,
        IsActive = category.IsActive
    };
}
