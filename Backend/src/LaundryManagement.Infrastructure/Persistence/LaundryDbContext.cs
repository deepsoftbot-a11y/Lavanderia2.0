using System;
using System.Collections.Generic;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryManagement.Infrastructure.Persistence;

public partial class LaundryDbContext : DbContext
{
    public LaundryDbContext()
    {
    }

    public LaundryDbContext(DbContextOptions<LaundryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditoriaGeneral> AuditoriaGenerals { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Combo> Combos { get; set; }

    public virtual DbSet<CombosDetalle> CombosDetalles { get; set; }

    public virtual DbSet<ConfiguracionReporte> ConfiguracionReportes { get; set; }

    public virtual DbSet<CortesCaja> CortesCajas { get; set; }

    public virtual DbSet<CortesCajaDetalle> CortesCajaDetalles { get; set; }

    public virtual DbSet<Descuento> Descuentos { get; set; }

    public virtual DbSet<EstadosOrden> EstadosOrdens { get; set; }

    public virtual DbSet<HistorialEstadosOrden> HistorialEstadosOrdens { get; set; }

    public virtual DbSet<HistorialReporte> HistorialReportes { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<Ordene> Ordenes { get; set; }

    public virtual DbSet<OrdenesDescuento> OrdenesDescuentos { get; set; }

    public virtual DbSet<OrdenesDetalle> OrdenesDetalles { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<PagosDetalle> PagosDetalles { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolesPermiso> RolesPermisos { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<ServiciosPrenda> ServiciosPrendas { get; set; }

    public virtual DbSet<TiposPrendum> TiposPrenda { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuariosRole> UsuariosRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Configuration will be provided via dependency injection
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditoriaGeneral>(entity =>
        {
            entity.HasKey(e => e.AuditoriaId);

            entity.ToTable("AuditoriaGeneral");

            entity.HasIndex(e => new { e.Tabla, e.FechaOperacion }, "IX_AuditoriaGeneral_Tabla_FechaOperacion").IsDescending(false, true);

            entity.Property(e => e.AuditoriaId).HasColumnName("AuditoriaID");
            entity.Property(e => e.DireccionIp)
                .HasMaxLength(45)
                .HasColumnName("DireccionIP");
            entity.Property(e => e.FechaOperacion)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.Operacion)
                .HasMaxLength(10);
            entity.Property(e => e.RegistroId).HasColumnName("RegistroID");
            entity.Property(e => e.Tabla)
                .HasMaxLength(100);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.ValoresAnteriores);
            entity.Property(e => e.ValoresNuevos);

            entity.HasOne(d => d.Usuario).WithMany(p => p.AuditoriaGenerals)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditoriaGeneral_UsuarioID");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(e => e.NombreCategoria, "UK_Categorias_NombreCategoria").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255);
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(e => e.NumeroCliente, "UK_Clientes_NumeroCliente").IsUnique();

            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Direccion)
                .HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.LimiteCredito).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(150);
            entity.Property(e => e.NumeroCliente)
                .HasMaxLength(20);
            entity.Property(e => e.Rfc)
                .HasMaxLength(13)
                .HasColumnName("RFC");
            entity.Property(e => e.SaldoActual).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20);

            entity.HasOne(d => d.RegistradoPorNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.RegistradoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_RegistradoPor");
        });

        modelBuilder.Entity<Combo>(entity =>
        {
            entity.Property(e => e.ComboId).HasColumnName("ComboID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255);
            entity.Property(e => e.NombreCombo)
                .HasMaxLength(100);
            entity.Property(e => e.PorcentajeDescuento).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<CombosDetalle>(entity =>
        {
            entity.HasKey(e => e.ComboDetalleId);

            entity.ToTable("CombosDetalle");

            entity.Property(e => e.ComboDetalleId).HasColumnName("ComboDetalleID");
            entity.Property(e => e.ComboId).HasColumnName("ComboID");
            entity.Property(e => e.ServicioPrendaId).HasColumnName("ServicioPrendaID");

            entity.HasOne(d => d.Combo).WithMany(p => p.CombosDetalles)
                .HasForeignKey(d => d.ComboId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CombosDetalle_ComboID");

            entity.HasOne(d => d.ServicioPrenda).WithMany(p => p.CombosDetalles)
                .HasForeignKey(d => d.ServicioPrendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CombosDetalle_ServicioPrendaID");
        });

        modelBuilder.Entity<ConfiguracionReporte>(entity =>
        {
            entity.HasKey(e => e.ConfigReporteId);

            entity.Property(e => e.ConfigReporteId).HasColumnName("ConfigReporteID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.DestinatariosEmail)
                .HasMaxLength(500);
            entity.Property(e => e.FormatoExportacion)
                .HasMaxLength(20);
            entity.Property(e => e.Frecuencia)
                .HasMaxLength(20);
            entity.Property(e => e.HoraEnvio)
                .HasMaxLength(5);
            entity.Property(e => e.NombreReporte)
                .HasMaxLength(100);
            entity.Property(e => e.ParametrosJson)
                .HasColumnName("ParametrosJSON");
            entity.Property(e => e.TipoReporte)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<CortesCaja>(entity =>
        {
            entity.HasKey(e => e.CorteId);

            entity.ToTable("CortesCaja");

            entity.HasIndex(e => e.CajeroId, "IX_CortesCaja_Cajero");

            entity.HasIndex(e => e.FechaCorte, "IX_CortesCaja_FechaCorte");

            entity.HasIndex(e => new { e.FechaInicio, e.FechaFin }, "IX_CortesCaja_FechaInicio");

            entity.HasIndex(e => e.FolioCorte, "UK_CortesCaja_Folio").IsUnique();

            entity.Property(e => e.CorteId).HasColumnName("CorteID");
            entity.Property(e => e.CajeroId).HasColumnName("CajeroID");
            entity.Property(e => e.DiferenciaFinal).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.DiferenciaInicial).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.DiferenciaInicialEfectivo).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.DiferenciaInicialOtros).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.DiferenciaInicialTarjeta).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.DiferenciaInicialTransferencia).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.FechaAjuste);
            entity.Property(e => e.FechaCorte)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.FechaFin);
            entity.Property(e => e.FechaInicio);
            entity.Property(e => e.FondoInicial).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FolioCorte)
                .HasMaxLength(30);
            entity.Property(e => e.MontoAjuste).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MotivoAjuste)
                .HasMaxLength(500);
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500);
            entity.Property(e => e.TotalDeclarado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalDeclaradoEfectivo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalDeclaradoOtros).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalDeclaradoTarjeta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalDeclaradoTransferencia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalEsperado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalEsperadoEfectivo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalEsperadoOtros).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalEsperadoTarjeta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalEsperadoTransferencia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TurnoDescripcion)
                .HasMaxLength(50);

            entity.HasOne(d => d.Cajero).WithMany(p => p.CortesCajas)
                .HasForeignKey(d => d.CajeroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CortesCaja_Cajero");
        });

        modelBuilder.Entity<CortesCajaDetalle>(entity =>
        {
            entity.HasKey(e => e.CorteDetalleId);

            entity.ToTable("CortesCajaDetalle");

            entity.Property(e => e.CorteDetalleId).HasColumnName("CorteDetalleID");
            entity.Property(e => e.CorteId).HasColumnName("CorteID");
            entity.Property(e => e.Diferencia).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.MetodoPagoId).HasColumnName("MetodoPagoID");
            entity.Property(e => e.TotalDeclarado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalEsperado).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Corte).WithMany(p => p.CortesCajaDetalles)
                .HasForeignKey(d => d.CorteId)
                .HasConstraintName("FK_CortesCajaDetalle_Corte");

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.CortesCajaDetalles)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CortesCajaDetalle_Metodo");
        });

        modelBuilder.Entity<Descuento>(entity =>
        {
            entity.Property(e => e.DescuentoId).HasColumnName("DescuentoID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.NombreDescuento)
                .HasMaxLength(100);
            entity.Property(e => e.TipoDescuento)
                .HasMaxLength(20);
            entity.Property(e => e.Valor).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<EstadosOrden>(entity =>
        {
            entity.HasKey(e => e.EstadoOrdenId);

            entity.ToTable("EstadosOrden");

            entity.HasIndex(e => e.NombreEstado, "UK_EstadosOrden_NombreEstado").IsUnique();

            entity.Property(e => e.EstadoOrdenId).HasColumnName("EstadoOrdenID");
            entity.Property(e => e.ColorEstado)
                .HasMaxLength(7);
            entity.Property(e => e.NombreEstado)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<HistorialEstadosOrden>(entity =>
        {
            entity.HasKey(e => e.HistorialEstadoId);

            entity.ToTable("HistorialEstadosOrden");

            entity.Property(e => e.HistorialEstadoId).HasColumnName("HistorialEstadoID");
            entity.Property(e => e.Comentarios)
                .HasMaxLength(500);
            entity.Property(e => e.EstadoOrdenId).HasColumnName("EstadoOrdenID");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.OrdenId).HasColumnName("OrdenID");

            entity.HasOne(d => d.CambiadoPorNavigation).WithMany(p => p.HistorialEstadosOrdens)
                .HasForeignKey(d => d.CambiadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialEstadosOrden_CambiadoPor");

            entity.HasOne(d => d.EstadoOrden).WithMany(p => p.HistorialEstadosOrdens)
                .HasForeignKey(d => d.EstadoOrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialEstadosOrden_EstadoOrdenID");

            entity.HasOne(d => d.Orden).WithMany(p => p.HistorialEstadosOrdens)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialEstadosOrden_OrdenID");
        });

        modelBuilder.Entity<HistorialReporte>(entity =>
        {
            entity.Property(e => e.HistorialReporteId).HasColumnName("HistorialReporteID");
            entity.Property(e => e.ConfigReporteId).HasColumnName("ConfigReporteID");
            entity.Property(e => e.Estado)
                .HasMaxLength(20);
            entity.Property(e => e.FechaEnvio);
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.MensajeError);
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500);

            entity.HasOne(d => d.ConfigReporte).WithMany(p => p.HistorialReportes)
                .HasForeignKey(d => d.ConfigReporteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialReportes_ConfigReporteID");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.MetodoPagoId);

            entity.ToTable("MetodosPago");

            entity.HasIndex(e => e.NombreMetodo, "UK_MetodosPago_NombreMetodo").IsUnique();

            entity.Property(e => e.MetodoPagoId).HasColumnName("MetodoPagoID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.NombreMetodo)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Ordene>(entity =>
        {
            entity.HasKey(e => e.OrdenId);

            entity.HasIndex(e => e.ClienteId, "IX_Ordenes_ClienteID");

            entity.HasIndex(e => e.EstadoOrdenId, "IX_Ordenes_EstadoOrdenID");

            entity.HasIndex(e => e.FechaRecepcion, "IX_Ordenes_FechaRecepcion").IsDescending();

            entity.HasIndex(e => e.FolioOrden, "UK_Ordenes_FolioOrden").IsUnique();

            entity.Property(e => e.OrdenId).HasColumnName("OrdenID");
            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.Descuento).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EstadoOrdenId).HasColumnName("EstadoOrdenID");
            entity.Property(e => e.FechaEntrega);
            entity.Property(e => e.FechaPrometida);
            entity.Property(e => e.FechaRecepcion)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.FolioOrden)
                .HasMaxLength(30);
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Ubicaciones)
                .HasMaxLength(500);

            entity.HasOne(d => d.Cliente).WithMany(p => p.Ordenes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ordenes_ClienteID");

            entity.HasOne(d => d.EntregadoPorNavigation).WithMany(p => p.OrdeneEntregadoPorNavigations)
                .HasForeignKey(d => d.EntregadoPor)
                .HasConstraintName("FK_Ordenes_EntregadoPor");

            entity.HasOne(d => d.EstadoOrden).WithMany(p => p.Ordenes)
                .HasForeignKey(d => d.EstadoOrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ordenes_EstadoOrdenID");

            entity.HasOne(d => d.RecibidoPorNavigation).WithMany(p => p.OrdeneRecibidoPorNavigations)
                .HasForeignKey(d => d.RecibidoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ordenes_RecibidoPor");
        });

        modelBuilder.Entity<OrdenesDescuento>(entity =>
        {
            entity.HasKey(e => e.OrdenDescuentoId);

            entity.Property(e => e.OrdenDescuentoId).HasColumnName("OrdenDescuentoID");
            entity.Property(e => e.ComboId).HasColumnName("ComboID");
            entity.Property(e => e.DescuentoId).HasColumnName("DescuentoID");
            entity.Property(e => e.Justificacion)
                .HasMaxLength(255);
            entity.Property(e => e.MontoDescuento).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrdenId).HasColumnName("OrdenID");

            entity.HasOne(d => d.AplicadoPorNavigation).WithMany(p => p.OrdenesDescuentos)
                .HasForeignKey(d => d.AplicadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenesDescuentos_AplicadoPor");

            entity.HasOne(d => d.Combo).WithMany(p => p.OrdenesDescuentos)
                .HasForeignKey(d => d.ComboId)
                .HasConstraintName("FK_OrdenesDescuentos_ComboID");

            entity.HasOne(d => d.Descuento).WithMany(p => p.OrdenesDescuentos)
                .HasForeignKey(d => d.DescuentoId)
                .HasConstraintName("FK_OrdenesDescuentos_DescuentoID");

            entity.HasOne(d => d.Orden).WithMany(p => p.OrdenesDescuentos)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenesDescuentos_OrdenID");
        });

        modelBuilder.Entity<OrdenesDetalle>(entity =>
        {
            entity.HasKey(e => e.OrdenDetalleId);

            entity.ToTable("OrdenesDetalle");

            entity.HasIndex(e => e.OrdenId, "IX_OrdenesDetalle_OrdenID");

            entity.HasIndex(e => e.ServicioId, "IX_OrdenesDetalle_ServicioID");

            entity.Property(e => e.OrdenDetalleId).HasColumnName("OrdenDetalleID");
            entity.Property(e => e.DescuentoLinea).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500);
            entity.Property(e => e.OrdenId).HasColumnName("OrdenID");
            entity.Property(e => e.PesoKilos).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServicioId).HasColumnName("ServicioID");
            entity.Property(e => e.ServicioPrendaId).HasColumnName("ServicioPrendaID");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalLinea).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Orden).WithMany(p => p.OrdenesDetalles)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenesDetalle_OrdenID");

            entity.HasOne(d => d.Servicio).WithMany(p => p.OrdenesDetalles)
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenesDetalle_ServicioID");

            entity.HasOne(d => d.ServicioPrenda).WithMany(p => p.OrdenesDetalles)
                .HasForeignKey(d => d.ServicioPrendaId)
                .HasConstraintName("FK_OrdenesDetalle_ServicioPrendaID");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasIndex(e => e.OrdenId, "IX_Pagos_OrdenID");

            entity.HasIndex(e => e.FolioPago, "UK_Pagos_FolioPago").IsUnique();

            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.FolioPago)
                .HasMaxLength(30);
            entity.Property(e => e.MontoPago).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500);
            entity.Property(e => e.OrdenId).HasColumnName("OrdenID");
            entity.Property(e => e.CanceladoEn);
            entity.Property(e => e.CanceladoPor).IsRequired(false);

            entity.HasOne(d => d.Orden).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_OrdenID");

            entity.HasOne(d => d.RecibioPorNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.RecibioPor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_RecibioPor");
        });

        modelBuilder.Entity<PagosDetalle>(entity =>
        {
            entity.HasKey(e => e.PagoDetalleId);

            entity.ToTable("PagosDetalle");

            entity.Property(e => e.PagoDetalleId).HasColumnName("PagoDetalleID");
            entity.Property(e => e.MetodoPagoId).HasColumnName("MetodoPagoID");
            entity.Property(e => e.MontoPagado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.Referencia)
                .HasMaxLength(100);

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.PagosDetalles)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagosDetalle_MetodoPagoID");

            entity.HasOne(d => d.Pago).WithMany(p => p.PagosDetalles)
                .HasForeignKey(d => d.PagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PagosDetalle_PagoID");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasIndex(e => e.Modulo, "IX_Permisos_Modulo");

            entity.HasIndex(e => e.NombrePermiso, "UK_Permisos_NombrePermiso").IsUnique();

            entity.Property(e => e.PermisoId).HasColumnName("PermisoID");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255);
            entity.Property(e => e.Modulo)
                .HasMaxLength(50);
            entity.Property(e => e.NombrePermiso)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId);

            entity.HasIndex(e => e.Activo, "IX_Roles_Activo");

            entity.HasIndex(e => e.NombreRol, "UK_Roles_NombreRol").IsUnique();

            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255);
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<RolesPermiso>(entity =>
        {
            entity.HasKey(e => e.RolPermisoId);

            entity.HasIndex(e => e.PermisoId, "IX_RolesPermisos_PermisoID");

            entity.HasIndex(e => e.RolId, "IX_RolesPermisos_RolID");

            entity.HasIndex(e => new { e.RolId, e.PermisoId }, "UK_RolesPermisos_Rol_Permiso").IsUnique();

            entity.Property(e => e.RolPermisoId).HasColumnName("RolPermisoID");
            entity.Property(e => e.PermisoId).HasColumnName("PermisoID");
            entity.Property(e => e.RolId).HasColumnName("RolID");

            entity.HasOne(d => d.Permiso).WithMany(p => p.RolesPermisos)
                .HasForeignKey(d => d.PermisoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolesPermisos_PermisoID");

            entity.HasOne(d => d.Rol).WithMany(p => p.RolesPermisos)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolesPermisos_RolID");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasIndex(e => e.Activo, "IX_Servicios_Activo");

            entity.HasIndex(e => e.CategoriaId, "IX_Servicios_CategoriaID");

            entity.HasIndex(e => e.TipoCobroServicio, "IX_Servicios_TipoCobroServicio");

            entity.HasIndex(e => e.CodigoServicio, "UK_Servicios_CodigoServicio").IsUnique();

            entity.Property(e => e.ServicioId).HasColumnName("ServicioID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.CodigoServicio)
                .HasMaxLength(20);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.NombreServicio)
                .HasMaxLength(100);
            entity.Property(e => e.PesoMaximo).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.PesoMinimo).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.PrecioPorKilo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TipoCobroServicio)
                .HasMaxLength(20);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Servicios_CategoriaID");
        });

        modelBuilder.Entity<ServiciosPrenda>(entity =>
        {
            entity.HasKey(e => e.ServicioPrendaId);

            entity.HasIndex(e => e.ServicioId, "IX_ServiciosPrendas_ServicioID");

            entity.HasIndex(e => e.TipoPrendaId, "IX_ServiciosPrendas_TipoPrendaID");

            entity.HasIndex(e => new { e.ServicioId, e.TipoPrendaId }, "UK_ServiciosPrendas_Servicio_Prenda").IsUnique();

            entity.Property(e => e.ServicioPrendaId).HasColumnName("ServicioPrendaID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServicioId).HasColumnName("ServicioID");
            entity.Property(e => e.TipoPrendaId).HasColumnName("TipoPrendaID");

            entity.HasOne(d => d.Servicio).WithMany(p => p.ServiciosPrenda)
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiciosPrendas_ServicioID");

            entity.HasOne(d => d.TipoPrenda).WithMany(p => p.ServiciosPrenda)
                .HasForeignKey(d => d.TipoPrendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiciosPrendas_TipoPrendaID");
        });

        modelBuilder.Entity<TiposPrendum>(entity =>
        {
            entity.HasKey(e => e.TipoPrendaId);

            entity.HasIndex(e => e.NombrePrenda, "UK_TiposPrenda_NombrePrenda").IsUnique();

            entity.Property(e => e.TipoPrendaId).HasColumnName("TipoPrendaID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255);
            entity.Property(e => e.NombrePrenda)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.Activo, "IX_Usuarios_Activo");

            entity.HasIndex(e => e.CreadoPor, "IX_Usuarios_CreadoPor").HasFilter("\"CreadoPor\" IS NOT NULL");

            entity.HasIndex(e => e.Email, "UK_Usuarios_Email").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "UK_Usuarios_NombreUsuario").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Email)
                .HasMaxLength(100);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(150);
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255);
            entity.Property(e => e.UltimoAcceso);

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.InverseCreadoPorNavigation)
                .HasForeignKey(d => d.CreadoPor)
                .HasConstraintName("FK_Usuarios_CreadoPor");
        });

        modelBuilder.Entity<UsuariosRole>(entity =>
        {
            entity.HasKey(e => e.UsuarioRolId);

            entity.HasIndex(e => e.RolId, "IX_UsuariosRoles_RolID");

            entity.HasIndex(e => e.UsuarioId, "IX_UsuariosRoles_UsuarioID");

            entity.HasIndex(e => new { e.UsuarioId, e.RolId }, "UK_UsuariosRoles_Usuario_Rol").IsUnique();

            entity.Property(e => e.UsuarioRolId).HasColumnName("UsuarioRolID");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.RolId).HasColumnName("RolID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Rol).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuariosRoles_RolID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuariosRoles_UsuarioID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
