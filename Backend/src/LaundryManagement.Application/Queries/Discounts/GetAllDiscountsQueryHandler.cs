using LaundryManagement.Application.Commands.Discounts;
using LaundryManagement.Application.DTOs.Discounts;
using LaundryManagement.Domain.Aggregates.Discounts;
using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.Discounts;

/// <summary>
/// Handler para obtener todos los descuentos con filtros y ordenamiento.
/// </summary>
public sealed class GetAllDiscountsQueryHandler : IRequestHandler<GetAllDiscountsQuery, List<DiscountDto>>
{
    private readonly IDiscountRepository _discountRepository;

    public GetAllDiscountsQueryHandler(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
    }

    public async Task<List<DiscountDto>> Handle(GetAllDiscountsQuery query, CancellationToken ct)
    {
        var discounts = await _discountRepository.GetAllAsync(ct);
        IEnumerable<DiscountPure> filtered = discounts;

        // Filtrar por tipo
        if (!string.IsNullOrWhiteSpace(query.Tipo))
        {
            var tipo = query.Tipo.Trim().ToUpperInvariant();
            filtered = filtered.Where(d => d.Type.ToString() == tipo);
        }

        // Filtrar por estado activo
        if (query.Activo.HasValue)
            filtered = filtered.Where(d => d.IsActive == query.Activo.Value);

        // Filtrar por búsqueda en nombre
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLowerInvariant();
            filtered = filtered.Where(d => d.Name.ToLowerInvariant().Contains(search));
        }

        // Mapear a DTOs
        var dtos = filtered.Select(CreateDiscountCommandHandler.MapToDto).ToList();

        // Ordenar
        dtos = (query.OrdenarPor.ToLowerInvariant(), query.Orden.ToLowerInvariant()) switch
        {
            ("name", "desc")  => dtos.OrderByDescending(d => d.Name).ToList(),
            ("name", _)       => dtos.OrderBy(d => d.Name).ToList(),
            ("value", "desc") => dtos.OrderByDescending(d => d.Value).ToList(),
            ("value", _)      => dtos.OrderBy(d => d.Value).ToList(),
            _                 => dtos.OrderBy(d => d.Name).ToList()
        };

        return dtos;
    }
}
