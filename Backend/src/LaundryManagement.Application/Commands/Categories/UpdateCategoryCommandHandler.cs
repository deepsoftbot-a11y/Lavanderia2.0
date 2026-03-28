using LaundryManagement.Application.DTOs.Categories;
using LaundryManagement.Application.Queries.Categories;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Categories;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(ICategoryRepository repository, ILogger<UpdateCategoryCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category: {CategoryId}", command.CategoryId);

        var category = await _repository.GetByIdAsync(CategoryId.From(command.CategoryId), cancellationToken)
            ?? throw new NotFoundException($"Categoría con ID {command.CategoryId} no encontrada");

        // Patch semántico: aplicar solo los campos recibidos
        if (command.Name != null)
            category.UpdateInfo(command.Name, command.Description ?? category.Description);

        if (command.IsActive.HasValue)
        {
            if (command.IsActive.Value) category.Activate();
            else category.Deactivate();
        }

        await _repository.UpdateAsync(category, cancellationToken);

        _logger.LogInformation("Category updated: {CategoryId}", command.CategoryId);
        return GetCategoriesQueryHandler.MapToDto(category);
    }
}
