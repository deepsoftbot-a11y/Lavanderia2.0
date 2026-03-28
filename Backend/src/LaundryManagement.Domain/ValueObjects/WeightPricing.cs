using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.Exceptions;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que encapsula la lógica de precios por peso.
/// Contiene el precio por kilogramo y los rangos de peso (mínimo/máximo).
/// </summary>
public sealed class WeightPricing : ValueObject
{
    /// <summary>
    /// Precio por kilogramo
    /// </summary>
    public Money PricePerKilo { get; }

    /// <summary>
    /// Peso mínimo (en kg) - opcional
    /// </summary>
    public decimal? MinimumWeight { get; }

    /// <summary>
    /// Peso máximo (en kg) - opcional
    /// </summary>
    public decimal? MaximumWeight { get; }

    /// <summary>
    /// Constructor privado
    /// </summary>
    private WeightPricing(Money pricePerKilo, decimal? minimumWeight, decimal? maximumWeight)
    {
        if (pricePerKilo.IsZero || pricePerKilo.Amount < 0)
            throw new ValidationException("El precio por kilogramo debe ser mayor que cero");

        if (minimumWeight.HasValue && minimumWeight.Value < 0)
            throw new ValidationException("El peso mínimo no puede ser negativo");

        if (maximumWeight.HasValue && maximumWeight.Value < 0)
            throw new ValidationException("El peso máximo no puede ser negativo");

        if (minimumWeight.HasValue && maximumWeight.HasValue && minimumWeight.Value > maximumWeight.Value)
            throw new ValidationException("El peso mínimo no puede ser mayor que el peso máximo");

        PricePerKilo = pricePerKilo;
        MinimumWeight = minimumWeight;
        MaximumWeight = maximumWeight;
    }

    /// <summary>
    /// Crea un WeightPricing con todos los parámetros
    /// </summary>
    public static WeightPricing Create(Money pricePerKilo, decimal? minimumWeight = null, decimal? maximumWeight = null)
    {
        return new WeightPricing(pricePerKilo, minimumWeight, maximumWeight);
    }

    /// <summary>
    /// Calcula el precio total basado en el peso dado
    /// </summary>
    /// <param name="weight">Peso en kilogramos</param>
    /// <returns>Precio total calculado</returns>
    public Money CalculatePrice(decimal weight)
    {
        if (weight < 0)
            throw new ValidationException("El peso no puede ser negativo");

        // Validar que el peso esté dentro del rango permitido
        if (MinimumWeight.HasValue && weight < MinimumWeight.Value)
            throw new BusinessRuleException($"El peso {weight} kg está por debajo del mínimo permitido de {MinimumWeight.Value} kg");

        if (MaximumWeight.HasValue && weight > MaximumWeight.Value)
            throw new BusinessRuleException($"El peso {weight} kg excede el máximo permitido de {MaximumWeight.Value} kg");

        // Calcular precio: peso * precio_por_kilo
        return PricePerKilo * weight;
    }

    /// <summary>
    /// Verifica si un peso está dentro del rango válido
    /// </summary>
    public bool IsWeightInRange(decimal weight)
    {
        if (weight < 0)
            return false;

        if (MinimumWeight.HasValue && weight < MinimumWeight.Value)
            return false;

        if (MaximumWeight.HasValue && weight > MaximumWeight.Value)
            return false;

        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PricePerKilo;
        yield return MinimumWeight;
        yield return MaximumWeight;
    }

    public override string ToString()
    {
        var range = "";
        if (MinimumWeight.HasValue || MaximumWeight.HasValue)
        {
            var min = MinimumWeight?.ToString() ?? "0";
            var max = MaximumWeight?.ToString() ?? "∞";
            range = $" (Rango: {min}-{max} kg)";
        }
        return $"{PricePerKilo}/kg{range}";
    }
}
