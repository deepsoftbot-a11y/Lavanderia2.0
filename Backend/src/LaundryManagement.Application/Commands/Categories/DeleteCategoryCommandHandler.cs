using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Application.Commands.Categories;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(ICategoryRepository repository, ILogger<DeleteCategoryCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category: {CategoryId}", command.CategoryId);

        var category = await _repository.GetByIdAsync(CategoryId.From(command.CategoryId), cancellationToken)
            ?? throw new NotFoundException($"Categoría con ID {command.CategoryId} no encontrada");

        await _repository.DeleteAsync(category.Id, cancellationToken);

        _logger.LogInformation("Category deleted: {CategoryId}", command.CategoryId);
        return Unit.Value;
    }
}
