using LaundryManagement.Domain.Common;

namespace LaundryManagement.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el identificador único de un cliente.
/// </summary>
public sealed class ClientId : ValueObject
{
    public int Value { get; }

    private ClientId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("El ClientId debe ser mayor a cero", nameof(value));

        Value = value;
    }

    public static ClientId From(int value) => new ClientId(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(ClientId clientId) => clientId.Value;
}
