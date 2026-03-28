using System;
using System.Collections.Generic;

namespace LaundryManagement.Infrastructure.Persistence.Entities;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public string NumeroCliente { get; set; } = null!;

    public string NombreCompleto { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public string? Rfc { get; set; }

    public decimal LimiteCredito { get; set; }

    public decimal SaldoActual { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int RegistradoPor { get; set; }

    public virtual ICollection<Ordene> Ordenes { get; set; } = new List<Ordene>();

    public virtual Usuario RegistradoPorNavigation { get; set; } = null!;
}
