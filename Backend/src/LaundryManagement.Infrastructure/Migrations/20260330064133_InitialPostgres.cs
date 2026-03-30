using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LaundryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    CategoriaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCategoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.CategoriaID);
                });

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    ComboID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCombo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PorcentajeDescuento = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.ComboID);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionReportes",
                columns: table => new
                {
                    ConfigReporteID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreReporte = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoReporte = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Frecuencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FormatoExportacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DestinatariosEmail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HoraEnvio = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ParametrosJSON = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionReportes", x => x.ConfigReporteID);
                });

            migrationBuilder.CreateTable(
                name: "Descuentos",
                columns: table => new
                {
                    DescuentoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreDescuento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoDescuento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descuentos", x => x.DescuentoID);
                });

            migrationBuilder.CreateTable(
                name: "EstadosOrden",
                columns: table => new
                {
                    EstadoOrdenID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreEstado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorEstado = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    OrdenProceso = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosOrden", x => x.EstadoOrdenID);
                });

            migrationBuilder.CreateTable(
                name: "MetodosPago",
                columns: table => new
                {
                    MetodoPagoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreMetodo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequiereReferencia = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.MetodoPagoID);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    PermisoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombrePermiso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.PermisoID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RolID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreRol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolID);
                });

            migrationBuilder.CreateTable(
                name: "TiposPrenda",
                columns: table => new
                {
                    TipoPrendaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombrePrenda = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPrenda", x => x.TipoPrendaID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreUsuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreadoPor = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioID);
                    table.ForeignKey(
                        name: "FK_Usuarios_CreadoPor",
                        column: x => x.CreadoPor,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    ServicioID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoriaID = table.Column<int>(type: "integer", nullable: false),
                    CodigoServicio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NombreServicio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoCobroServicio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PrecioPorKilo = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PesoMinimo = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    PesoMaximo = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TiempoEstimado = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.ServicioID);
                    table.ForeignKey(
                        name: "FK_Servicios_CategoriaID",
                        column: x => x.CategoriaID,
                        principalTable: "Categorias",
                        principalColumn: "CategoriaID");
                });

            migrationBuilder.CreateTable(
                name: "HistorialReportes",
                columns: table => new
                {
                    HistorialReporteID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigReporteID = table.Column<int>(type: "integer", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RutaArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MensajeError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialReportes", x => x.HistorialReporteID);
                    table.ForeignKey(
                        name: "FK_HistorialReportes_ConfigReporteID",
                        column: x => x.ConfigReporteID,
                        principalTable: "ConfiguracionReportes",
                        principalColumn: "ConfigReporteID");
                });

            migrationBuilder.CreateTable(
                name: "RolesPermisos",
                columns: table => new
                {
                    RolPermisoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RolID = table.Column<int>(type: "integer", nullable: false),
                    PermisoID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermisos", x => x.RolPermisoID);
                    table.ForeignKey(
                        name: "FK_RolesPermisos_PermisoID",
                        column: x => x.PermisoID,
                        principalTable: "Permisos",
                        principalColumn: "PermisoID");
                    table.ForeignKey(
                        name: "FK_RolesPermisos_RolID",
                        column: x => x.RolID,
                        principalTable: "Roles",
                        principalColumn: "RolID");
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaGeneral",
                columns: table => new
                {
                    AuditoriaID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tabla = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operacion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RegistroID = table.Column<int>(type: "integer", nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "text", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "text", nullable: true),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    FechaOperacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DireccionIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaGeneral", x => x.AuditoriaID);
                    table.ForeignKey(
                        name: "FK_AuditoriaGeneral_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ClienteID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RFC = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    LimiteCredito = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    SaldoActual = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    RegistradoPor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ClienteID);
                    table.ForeignKey(
                        name: "FK_Clientes_RegistradoPor",
                        column: x => x.RegistradoPor,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "CortesCaja",
                columns: table => new
                {
                    CorteID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioCorte = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CajeroID = table.Column<int>(type: "integer", nullable: false),
                    TurnoDescripcion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCorte = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FondoInicial = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    TotalEsperadoEfectivo = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalEsperadoTarjeta = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalEsperadoTransferencia = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalEsperadoOtros = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalEsperado = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalDeclaradoEfectivo = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalDeclaradoTarjeta = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalDeclaradoTransferencia = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalDeclaradoOtros = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalDeclarado = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DiferenciaInicialEfectivo = table.Column<decimal>(type: "numeric(11,2)", nullable: true),
                    DiferenciaInicialTarjeta = table.Column<decimal>(type: "numeric(11,2)", nullable: true),
                    DiferenciaInicialTransferencia = table.Column<decimal>(type: "numeric(11,2)", nullable: true),
                    DiferenciaInicialOtros = table.Column<decimal>(type: "numeric(11,2)", nullable: true),
                    DiferenciaInicial = table.Column<decimal>(type: "numeric(11,2)", nullable: true),
                    MontoAjuste = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MotivoAjuste = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaAjuste = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DiferenciaFinal = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    NumeroTransacciones = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CortesCaja", x => x.CorteID);
                    table.ForeignKey(
                        name: "FK_CortesCaja_Cajero",
                        column: x => x.CajeroID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                columns: table => new
                {
                    UsuarioRolID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    RolID = table.Column<int>(type: "integer", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => x.UsuarioRolID);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_RolID",
                        column: x => x.RolID,
                        principalTable: "Roles",
                        principalColumn: "RolID");
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "ServiciosPrendas",
                columns: table => new
                {
                    ServicioPrendaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServicioID = table.Column<int>(type: "integer", nullable: false),
                    TipoPrendaID = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosPrendas", x => x.ServicioPrendaID);
                    table.ForeignKey(
                        name: "FK_ServiciosPrendas_ServicioID",
                        column: x => x.ServicioID,
                        principalTable: "Servicios",
                        principalColumn: "ServicioID");
                    table.ForeignKey(
                        name: "FK_ServiciosPrendas_TipoPrendaID",
                        column: x => x.TipoPrendaID,
                        principalTable: "TiposPrenda",
                        principalColumn: "TipoPrendaID");
                });

            migrationBuilder.CreateTable(
                name: "Ordenes",
                columns: table => new
                {
                    OrdenID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioOrden = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ClienteID = table.Column<int>(type: "integer", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FechaPrometida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstadoOrdenID = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ubicaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecibidoPor = table.Column<int>(type: "integer", nullable: false),
                    EntregadoPor = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordenes", x => x.OrdenID);
                    table.ForeignKey(
                        name: "FK_Ordenes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "ClienteID");
                    table.ForeignKey(
                        name: "FK_Ordenes_EntregadoPor",
                        column: x => x.EntregadoPor,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                    table.ForeignKey(
                        name: "FK_Ordenes_EstadoOrdenID",
                        column: x => x.EstadoOrdenID,
                        principalTable: "EstadosOrden",
                        principalColumn: "EstadoOrdenID");
                    table.ForeignKey(
                        name: "FK_Ordenes_RecibidoPor",
                        column: x => x.RecibidoPor,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "CortesCajaDetalle",
                columns: table => new
                {
                    CorteDetalleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CorteID = table.Column<int>(type: "integer", nullable: false),
                    MetodoPagoID = table.Column<int>(type: "integer", nullable: false),
                    NumeroTransacciones = table.Column<int>(type: "integer", nullable: false),
                    TotalEsperado = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalDeclarado = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Diferencia = table.Column<decimal>(type: "numeric(11,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CortesCajaDetalle", x => x.CorteDetalleID);
                    table.ForeignKey(
                        name: "FK_CortesCajaDetalle_Corte",
                        column: x => x.CorteID,
                        principalTable: "CortesCaja",
                        principalColumn: "CorteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CortesCajaDetalle_Metodo",
                        column: x => x.MetodoPagoID,
                        principalTable: "MetodosPago",
                        principalColumn: "MetodoPagoID");
                });

            migrationBuilder.CreateTable(
                name: "CombosDetalle",
                columns: table => new
                {
                    ComboDetalleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComboID = table.Column<int>(type: "integer", nullable: false),
                    ServicioPrendaID = table.Column<int>(type: "integer", nullable: false),
                    CantidadMinima = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombosDetalle", x => x.ComboDetalleID);
                    table.ForeignKey(
                        name: "FK_CombosDetalle_ComboID",
                        column: x => x.ComboID,
                        principalTable: "Combos",
                        principalColumn: "ComboID");
                    table.ForeignKey(
                        name: "FK_CombosDetalle_ServicioPrendaID",
                        column: x => x.ServicioPrendaID,
                        principalTable: "ServiciosPrendas",
                        principalColumn: "ServicioPrendaID");
                });

            migrationBuilder.CreateTable(
                name: "HistorialEstadosOrden",
                columns: table => new
                {
                    HistorialEstadoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdenID = table.Column<int>(type: "integer", nullable: false),
                    EstadoOrdenID = table.Column<int>(type: "integer", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CambiadoPor = table.Column<int>(type: "integer", nullable: false),
                    Comentarios = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadosOrden", x => x.HistorialEstadoID);
                    table.ForeignKey(
                        name: "FK_HistorialEstadosOrden_CambiadoPor",
                        column: x => x.CambiadoPor,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                    table.ForeignKey(
                        name: "FK_HistorialEstadosOrden_EstadoOrdenID",
                        column: x => x.EstadoOrdenID,
                        principalTable: "EstadosOrden",
                        principalColumn: "EstadoOrdenID");
                    table.ForeignKey(
                        name: "FK_HistorialEstadosOrden_OrdenID",
                        column: x => x.OrdenID,
                        principalTable: "Ordenes",
                        principalColumn: "OrdenID");
                });

            migrationBuilder.CreateTable(
                name: "OrdenesDescuentos",
                columns: table => new
                {
                    OrdenDescuentoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdenID = table.Column<int>(type: "integer", nullable: false),
                    DescuentoID = table.Column<int>(type: "integer", nullable: true),
                    ComboID = table.Column<int>(type: "integer", nullable: true),
                    MontoDescuento = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Justificacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AplicadoPor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesDescuentos", x => x.OrdenDescuentoID);
                    table.ForeignKey(
                        name: "FK_OrdenesDescuentos_AplicadoPor",
                        column: x => x.AplicadoPor,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                    table.ForeignKey(
                        name: "FK_OrdenesDescuentos_ComboID",
                        column: x => x.ComboID,
                        principalTable: "Combos",
                        principalColumn: "ComboID");
                    table.ForeignKey(
                        name: "FK_OrdenesDescuentos_DescuentoID",
                        column: x => x.DescuentoID,
                        principalTable: "Descuentos",
                        principalColumn: "DescuentoID");
                    table.ForeignKey(
                        name: "FK_OrdenesDescuentos_OrdenID",
                        column: x => x.OrdenID,
                        principalTable: "Ordenes",
                        principalColumn: "OrdenID");
                });

            migrationBuilder.CreateTable(
                name: "OrdenesDetalle",
                columns: table => new
                {
                    OrdenDetalleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdenID = table.Column<int>(type: "integer", nullable: false),
                    NumeroLinea = table.Column<int>(type: "integer", nullable: false),
                    ServicioID = table.Column<int>(type: "integer", nullable: false),
                    ServicioPrendaID = table.Column<int>(type: "integer", nullable: true),
                    PesoKilos = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DescuentoLinea = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalLinea = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesDetalle", x => x.OrdenDetalleID);
                    table.ForeignKey(
                        name: "FK_OrdenesDetalle_OrdenID",
                        column: x => x.OrdenID,
                        principalTable: "Ordenes",
                        principalColumn: "OrdenID");
                    table.ForeignKey(
                        name: "FK_OrdenesDetalle_ServicioID",
                        column: x => x.ServicioID,
                        principalTable: "Servicios",
                        principalColumn: "ServicioID");
                    table.ForeignKey(
                        name: "FK_OrdenesDetalle_ServicioPrendaID",
                        column: x => x.ServicioPrendaID,
                        principalTable: "ServiciosPrendas",
                        principalColumn: "ServicioPrendaID");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolioPago = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrdenID = table.Column<int>(type: "integer", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    MontoPago = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    RecibioPor = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CanceladoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanceladoPor = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoID);
                    table.ForeignKey(
                        name: "FK_Pagos_OrdenID",
                        column: x => x.OrdenID,
                        principalTable: "Ordenes",
                        principalColumn: "OrdenID");
                    table.ForeignKey(
                        name: "FK_Pagos_RecibioPor",
                        column: x => x.RecibioPor,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "PagosDetalle",
                columns: table => new
                {
                    PagoDetalleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PagoID = table.Column<int>(type: "integer", nullable: false),
                    MetodoPagoID = table.Column<int>(type: "integer", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosDetalle", x => x.PagoDetalleID);
                    table.ForeignKey(
                        name: "FK_PagosDetalle_MetodoPagoID",
                        column: x => x.MetodoPagoID,
                        principalTable: "MetodosPago",
                        principalColumn: "MetodoPagoID");
                    table.ForeignKey(
                        name: "FK_PagosDetalle_PagoID",
                        column: x => x.PagoID,
                        principalTable: "Pagos",
                        principalColumn: "PagoID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaGeneral_Tabla_FechaOperacion",
                table: "AuditoriaGeneral",
                columns: new[] { "Tabla", "FechaOperacion" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaGeneral_UsuarioID",
                table: "AuditoriaGeneral",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "UK_Categorias_NombreCategoria",
                table: "Categorias",
                column: "NombreCategoria",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_RegistradoPor",
                table: "Clientes",
                column: "RegistradoPor");

            migrationBuilder.CreateIndex(
                name: "UK_Clientes_NumeroCliente",
                table: "Clientes",
                column: "NumeroCliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CombosDetalle_ComboID",
                table: "CombosDetalle",
                column: "ComboID");

            migrationBuilder.CreateIndex(
                name: "IX_CombosDetalle_ServicioPrendaID",
                table: "CombosDetalle",
                column: "ServicioPrendaID");

            migrationBuilder.CreateIndex(
                name: "IX_CortesCaja_Cajero",
                table: "CortesCaja",
                column: "CajeroID");

            migrationBuilder.CreateIndex(
                name: "IX_CortesCaja_FechaCorte",
                table: "CortesCaja",
                column: "FechaCorte");

            migrationBuilder.CreateIndex(
                name: "IX_CortesCaja_FechaInicio",
                table: "CortesCaja",
                columns: new[] { "FechaInicio", "FechaFin" });

            migrationBuilder.CreateIndex(
                name: "UK_CortesCaja_Folio",
                table: "CortesCaja",
                column: "FolioCorte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CortesCajaDetalle_CorteID",
                table: "CortesCajaDetalle",
                column: "CorteID");

            migrationBuilder.CreateIndex(
                name: "IX_CortesCajaDetalle_MetodoPagoID",
                table: "CortesCajaDetalle",
                column: "MetodoPagoID");

            migrationBuilder.CreateIndex(
                name: "UK_EstadosOrden_NombreEstado",
                table: "EstadosOrden",
                column: "NombreEstado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosOrden_CambiadoPor",
                table: "HistorialEstadosOrden",
                column: "CambiadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosOrden_EstadoOrdenID",
                table: "HistorialEstadosOrden",
                column: "EstadoOrdenID");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosOrden_OrdenID",
                table: "HistorialEstadosOrden",
                column: "OrdenID");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialReportes_ConfigReporteID",
                table: "HistorialReportes",
                column: "ConfigReporteID");

            migrationBuilder.CreateIndex(
                name: "UK_MetodosPago_NombreMetodo",
                table: "MetodosPago",
                column: "NombreMetodo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_ClienteID",
                table: "Ordenes",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_EntregadoPor",
                table: "Ordenes",
                column: "EntregadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_EstadoOrdenID",
                table: "Ordenes",
                column: "EstadoOrdenID");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_FechaRecepcion",
                table: "Ordenes",
                column: "FechaRecepcion",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_RecibidoPor",
                table: "Ordenes",
                column: "RecibidoPor");

            migrationBuilder.CreateIndex(
                name: "UK_Ordenes_FolioOrden",
                table: "Ordenes",
                column: "FolioOrden",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDescuentos_AplicadoPor",
                table: "OrdenesDescuentos",
                column: "AplicadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDescuentos_ComboID",
                table: "OrdenesDescuentos",
                column: "ComboID");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDescuentos_DescuentoID",
                table: "OrdenesDescuentos",
                column: "DescuentoID");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDescuentos_OrdenID",
                table: "OrdenesDescuentos",
                column: "OrdenID");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDetalle_OrdenID",
                table: "OrdenesDetalle",
                column: "OrdenID");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDetalle_ServicioID",
                table: "OrdenesDetalle",
                column: "ServicioID");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDetalle_ServicioPrendaID",
                table: "OrdenesDetalle",
                column: "ServicioPrendaID");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_OrdenID",
                table: "Pagos",
                column: "OrdenID");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_RecibioPor",
                table: "Pagos",
                column: "RecibioPor");

            migrationBuilder.CreateIndex(
                name: "UK_Pagos_FolioPago",
                table: "Pagos",
                column: "FolioPago",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagosDetalle_MetodoPagoID",
                table: "PagosDetalle",
                column: "MetodoPagoID");

            migrationBuilder.CreateIndex(
                name: "IX_PagosDetalle_PagoID",
                table: "PagosDetalle",
                column: "PagoID");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Modulo",
                table: "Permisos",
                column: "Modulo");

            migrationBuilder.CreateIndex(
                name: "UK_Permisos_NombrePermiso",
                table: "Permisos",
                column: "NombrePermiso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Activo",
                table: "Roles",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "UK_Roles_NombreRol",
                table: "Roles",
                column: "NombreRol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_PermisoID",
                table: "RolesPermisos",
                column: "PermisoID");

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_RolID",
                table: "RolesPermisos",
                column: "RolID");

            migrationBuilder.CreateIndex(
                name: "UK_RolesPermisos_Rol_Permiso",
                table: "RolesPermisos",
                columns: new[] { "RolID", "PermisoID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_Activo",
                table: "Servicios",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_CategoriaID",
                table: "Servicios",
                column: "CategoriaID");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_TipoCobroServicio",
                table: "Servicios",
                column: "TipoCobroServicio");

            migrationBuilder.CreateIndex(
                name: "UK_Servicios_CodigoServicio",
                table: "Servicios",
                column: "CodigoServicio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosPrendas_ServicioID",
                table: "ServiciosPrendas",
                column: "ServicioID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosPrendas_TipoPrendaID",
                table: "ServiciosPrendas",
                column: "TipoPrendaID");

            migrationBuilder.CreateIndex(
                name: "UK_ServiciosPrendas_Servicio_Prenda",
                table: "ServiciosPrendas",
                columns: new[] { "ServicioID", "TipoPrendaID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_TiposPrenda_NombrePrenda",
                table: "TiposPrenda",
                column: "NombrePrenda",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Activo",
                table: "Usuarios",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CreadoPor",
                table: "Usuarios",
                column: "CreadoPor",
                filter: "\"CreadoPor\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UK_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_RolID",
                table: "UsuariosRoles",
                column: "RolID");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_UsuarioID",
                table: "UsuariosRoles",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "UK_UsuariosRoles_Usuario_Rol",
                table: "UsuariosRoles",
                columns: new[] { "UsuarioID", "RolID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaGeneral");

            migrationBuilder.DropTable(
                name: "CombosDetalle");

            migrationBuilder.DropTable(
                name: "CortesCajaDetalle");

            migrationBuilder.DropTable(
                name: "HistorialEstadosOrden");

            migrationBuilder.DropTable(
                name: "HistorialReportes");

            migrationBuilder.DropTable(
                name: "OrdenesDescuentos");

            migrationBuilder.DropTable(
                name: "OrdenesDetalle");

            migrationBuilder.DropTable(
                name: "PagosDetalle");

            migrationBuilder.DropTable(
                name: "RolesPermisos");

            migrationBuilder.DropTable(
                name: "UsuariosRoles");

            migrationBuilder.DropTable(
                name: "CortesCaja");

            migrationBuilder.DropTable(
                name: "ConfiguracionReportes");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "Descuentos");

            migrationBuilder.DropTable(
                name: "ServiciosPrendas");

            migrationBuilder.DropTable(
                name: "MetodosPago");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "TiposPrenda");

            migrationBuilder.DropTable(
                name: "Ordenes");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "EstadosOrden");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
