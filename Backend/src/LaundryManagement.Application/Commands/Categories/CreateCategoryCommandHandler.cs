using LaundryManagement.Application.DTOs.Categories;
using LaundryManagement.Application.Queries.Categories;
using LaundryManagement.Domain.Aggregates.Categories;
using LaundryManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Categories;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(ICategoryRepository repository, ILogger<CreateCategoryCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category: {Name}", command.Name);

        var category = CategoryPure.Create(command.Name, command.Description);
        var saved = await _repository.AddAsync(category, cancellationToken);

        _logger.LogInformation("Category created: {CategoryId}", saved.Id.Value);
        return GetCategoriesQueryHandler.MapToDto(saved);
    }
}
