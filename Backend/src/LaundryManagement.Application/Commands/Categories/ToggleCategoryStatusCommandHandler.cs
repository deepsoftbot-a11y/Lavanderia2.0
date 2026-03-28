using LaundryManagement.Application.DTOs.Categories;
using LaundryManagement.Application.Queries.Categories;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Categories;

public sealed class ToggleCategoryStatusCommandHandler : IRequestHandler<ToggleCategoryStatusCommand, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<ToggleCategoryStatusCommandHandler> _logger;

    public ToggleCategoryStatusCommandHandler(ICategoryRepository repository, ILogger<ToggleCategoryStatusCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(ToggleCategoryStatusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Toggling category status: {CategoryId}", command.CategoryId);

        var category = await _repository.GetByIdAsync(CategoryId.From(command.CategoryId), cancellationToken)
            ?? throw new NotFoundException($"Categoría con ID {command.CategoryId} no encontrada");

        if (category.IsActive) category.Deactivate();
        else category.Activate();

        await _repository.UpdateAsync(category, cancellationToken);

        _logger.LogInformation("Category status toggled: {CategoryId} IsActive={IsActive}", command.CategoryId, category.IsActive);
        return GetCategoriesQueryHandler.MapToDto(category);
    }
}
