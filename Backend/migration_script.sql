IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Categorias] (
        [CategoriaID] int NOT NULL IDENTITY,
        [NombreCategoria] varchar(100) NOT NULL,
        [Descripcion] varchar(255) NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([CategoriaID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Combos] (
        [ComboID] int NOT NULL IDENTITY,
        [NombreCombo] varchar(100) NOT NULL,
        [Descripcion] varchar(255) NULL,
        [PorcentajeDescuento] decimal(5,2) NOT NULL,
        [FechaInicio] date NOT NULL,
        [FechaFin] date NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Combos] PRIMARY KEY ([ComboID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [ConfiguracionReportes] (
        [ConfigReporteID] int NOT NULL IDENTITY,
        [NombreReporte] varchar(100) NOT NULL,
        [TipoReporte] varchar(50) NOT NULL,
        [Frecuencia] varchar(20) NOT NULL,
        [FormatoExportacion] varchar(20) NOT NULL,
        [DestinatariosEmail] varchar(500) NULL,
        [HoraEnvio] varchar(5) NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [ParametrosJSON] varchar(max) NULL,
        CONSTRAINT [PK_ConfiguracionReportes] PRIMARY KEY ([ConfigReporteID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Descuentos] (
        [DescuentoID] int NOT NULL IDENTITY,
        [NombreDescuento] varchar(100) NOT NULL,
        [TipoDescuento] varchar(20) NOT NULL,
        [Valor] decimal(10,2) NOT NULL,
        [FechaInicio] date NOT NULL,
        [FechaFin] date NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Descuentos] PRIMARY KEY ([DescuentoID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [EstadosOrden] (
        [EstadoOrdenID] int NOT NULL IDENTITY,
        [NombreEstado] varchar(50) NOT NULL,
        [ColorEstado] varchar(7) NULL,
        [OrdenProceso] int NOT NULL,
        CONSTRAINT [PK_EstadosOrden] PRIMARY KEY ([EstadoOrdenID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [MetodosPago] (
        [MetodoPagoID] int NOT NULL IDENTITY,
        [NombreMetodo] varchar(50) NOT NULL,
        [RequiereReferencia] bit NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_MetodosPago] PRIMARY KEY ([MetodoPagoID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Permisos] (
        [PermisoID] int NOT NULL IDENTITY,
        [NombrePermiso] varchar(100) NOT NULL,
        [Modulo] varchar(50) NOT NULL,
        [Descripcion] varchar(255) NULL,
        CONSTRAINT [PK_Permisos] PRIMARY KEY ([PermisoID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Roles] (
        [RolID] int NOT NULL IDENTITY,
        [NombreRol] varchar(50) NOT NULL,
        [Descripcion] varchar(255) NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Roles] PRIMARY KEY ([RolID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [TiposPrenda] (
        [TipoPrendaID] int NOT NULL IDENTITY,
        [NombrePrenda] varchar(100) NOT NULL,
        [Descripcion] varchar(255) NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_TiposPrenda] PRIMARY KEY ([TipoPrendaID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [UsuarioID] int NOT NULL IDENTITY,
        [NombreUsuario] varchar(50) NOT NULL,
        [Email] varchar(100) NOT NULL,
        [PasswordHash] varchar(255) NOT NULL,
        [NombreCompleto] varchar(150) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
        [UltimoAcceso] datetime NULL,
        [CreadoPor] int NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([UsuarioID]),
        CONSTRAINT [FK_Usuarios_CreadoPor] FOREIGN KEY ([CreadoPor]) REFERENCES [Usuarios] ([UsuarioID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Servicios] (
        [ServicioID] int NOT NULL IDENTITY,
        [CategoriaID] int NOT NULL,
        [CodigoServicio] varchar(20) NOT NULL,
        [NombreServicio] varchar(100) NOT NULL,
        [TipoCobroServicio] varchar(20) NOT NULL,
        [PrecioPorKilo] decimal(10,2) NULL,
        [PesoMinimo] decimal(6,2) NULL,
        [PesoMaximo] decimal(6,2) NULL,
        [Descripcion] varchar(500) NULL,
        [TiempoEstimado] int NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime NOT NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_Servicios] PRIMARY KEY ([ServicioID]),
        CONSTRAINT [FK_Servicios_CategoriaID] FOREIGN KEY ([CategoriaID]) REFERENCES [Categorias] ([CategoriaID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [HistorialReportes] (
        [HistorialReporteID] int NOT NULL IDENTITY,
        [ConfigReporteID] int NOT NULL,
        [FechaGeneracion] datetime NOT NULL DEFAULT ((getdate())),
        [FechaEnvio] datetime NULL,
        [Estado] varchar(20) NOT NULL,
        [RutaArchivo] varchar(500) NULL,
        [MensajeError] varchar(max) NULL,
        CONSTRAINT [PK_HistorialReportes] PRIMARY KEY ([HistorialReporteID]),
        CONSTRAINT [FK_HistorialReportes_ConfigReporteID] FOREIGN KEY ([ConfigReporteID]) REFERENCES [ConfiguracionReportes] ([ConfigReporteID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [RolesPermisos] (
        [RolPermisoID] int NOT NULL IDENTITY,
        [RolID] int NOT NULL,
        [PermisoID] int NOT NULL,
        CONSTRAINT [PK_RolesPermisos] PRIMARY KEY ([RolPermisoID]),
        CONSTRAINT [FK_RolesPermisos_PermisoID] FOREIGN KEY ([PermisoID]) REFERENCES [Permisos] ([PermisoID]),
        CONSTRAINT [FK_RolesPermisos_RolID] FOREIGN KEY ([RolID]) REFERENCES [Roles] ([RolID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [AuditoriaGeneral] (
        [AuditoriaID] bigint NOT NULL IDENTITY,
        [Tabla] varchar(100) NOT NULL,
        [Operacion] varchar(10) NOT NULL,
        [RegistroID] int NOT NULL,
        [ValoresAnteriores] varchar(max) NULL,
        [ValoresNuevos] varchar(max) NULL,
        [UsuarioID] int NOT NULL,
        [FechaOperacion] datetime NOT NULL DEFAULT ((getdate())),
        [DireccionIP] varchar(45) NULL,
        CONSTRAINT [PK_AuditoriaGeneral] PRIMARY KEY ([AuditoriaID]),
        CONSTRAINT [FK_AuditoriaGeneral_UsuarioID] FOREIGN KEY ([UsuarioID]) REFERENCES [Usuarios] ([UsuarioID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Clientes] (
        [ClienteID] int NOT NULL IDENTITY,
        [NumeroCliente] varchar(20) NOT NULL,
        [NombreCompleto] varchar(150) NOT NULL,
        [Telefono] varchar(20) NOT NULL,
        [Email] varchar(100) NULL,
        [Direccion] varchar(255) NULL,
        [RFC] varchar(13) NULL,
        [LimiteCredito] decimal(10,2) NOT NULL,
        [SaldoActual] decimal(10,2) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaRegistro] datetime NOT NULL DEFAULT ((getdate())),
        [RegistradoPor] int NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([ClienteID]),
        CONSTRAINT [FK_Clientes_RegistradoPor] FOREIGN KEY ([RegistradoPor]) REFERENCES [Usuarios] ([UsuarioID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [CortesCaja] (
        [CorteID] int NOT NULL IDENTITY,
        [FolioCorte] varchar(30) NOT NULL,
        [CajeroID] int NOT NULL,
        [TurnoDescripcion] varchar(50) NULL,
        [FechaInicio] datetime NOT NULL,
        [FechaFin] datetime NOT NULL,
        [FechaCorte] datetime NOT NULL DEFAULT ((getdate())),
        [TotalEsperadoEfectivo] decimal(10,2) NOT NULL,
        [TotalEsperadoTarjeta] decimal(10,2) NOT NULL,
        [TotalEsperadoTransferencia] decimal(10,2) NOT NULL,
        [TotalEsperadoOtros] decimal(10,2) NOT NULL,
        [TotalEsperado] decimal(10,2) NOT NULL,
        [TotalDeclaradoEfectivo] decimal(10,2) NOT NULL,
        [TotalDeclaradoTarjeta] decimal(10,2) NOT NULL,
        [TotalDeclaradoTransferencia] decimal(10,2) NOT NULL,
        [TotalDeclaradoOtros] decimal(10,2) NOT NULL,
        [TotalDeclarado] decimal(10,2) NOT NULL,
        [DiferenciaInicialEfectivo] AS ([TotalDeclaradoEfectivo]-[TotalEsperadoEfectivo]) PERSISTED,
        [DiferenciaInicialTarjeta] AS ([TotalDeclaradoTarjeta]-[TotalEsperadoTarjeta]) PERSISTED,
        [DiferenciaInicialTransferencia] AS ([TotalDeclaradoTransferencia]-[TotalEsperadoTransferencia]) PERSISTED,
        [DiferenciaInicialOtros] AS ([TotalDeclaradoOtros]-[TotalEsperadoOtros]) PERSISTED,
        [DiferenciaInicial] AS ([TotalDeclarado]-[TotalEsperado]) PERSISTED,
        [MontoAjuste] decimal(10,2) NOT NULL,
        [MotivoAjuste] varchar(500) NULL,
        [FechaAjuste] datetime NULL,
        [DiferenciaFinal] AS (([TotalDeclarado]-[TotalEsperado])+[MontoAjuste]) PERSISTED,
        [NumeroTransacciones] int NOT NULL,
        [Observaciones] varchar(500) NULL,
        CONSTRAINT [PK_CortesCaja] PRIMARY KEY ([CorteID]),
        CONSTRAINT [FK_CortesCaja_Cajero] FOREIGN KEY ([CajeroID]) REFERENCES [Usuarios] ([UsuarioID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [UsuariosRoles] (
        [UsuarioRolID] int NOT NULL IDENTITY,
        [UsuarioID] int NOT NULL,
        [RolID] int NOT NULL,
        [FechaAsignacion] datetime NOT NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_UsuariosRoles] PRIMARY KEY ([UsuarioRolID]),
        CONSTRAINT [FK_UsuariosRoles_RolID] FOREIGN KEY ([RolID]) REFERENCES [Roles] ([RolID]),
        CONSTRAINT [FK_UsuariosRoles_UsuarioID] FOREIGN KEY ([UsuarioID]) REFERENCES [Usuarios] ([UsuarioID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [ServiciosPrendas] (
        [ServicioPrendaID] int NOT NULL IDENTITY,
        [ServicioID] int NOT NULL,
        [TipoPrendaID] int NOT NULL,
        [PrecioUnitario] decimal(10,2) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaActualizacion] datetime NOT NULL DEFAULT ((getdate())),
        CONSTRAINT [PK_ServiciosPrendas] PRIMARY KEY ([ServicioPrendaID]),
        CONSTRAINT [FK_ServiciosPrendas_ServicioID] FOREIGN KEY ([ServicioID]) REFERENCES [Servicios] ([ServicioID]),
        CONSTRAINT [FK_ServiciosPrendas_TipoPrendaID] FOREIGN KEY ([TipoPrendaID]) REFERENCES [TiposPrenda] ([TipoPrendaID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Ordenes] (
        [OrdenID] int NOT NULL IDENTITY,
        [FolioOrden] varchar(30) NOT NULL,
        [ClienteID] int NOT NULL,
        [FechaRecepcion] datetime NOT NULL DEFAULT ((getdate())),
        [FechaPrometida] datetime NOT NULL,
        [FechaEntrega] datetime NULL,
        [EstadoOrdenID] int NOT NULL,
        [Subtotal] decimal(10,2) NOT NULL,
        [Descuento] decimal(10,2) NOT NULL,
        [Total] decimal(10,2) NOT NULL,
        [Observaciones] varchar(500) NULL,
        [Ubicaciones] nvarchar(500) NULL,
        [RecibidoPor] int NOT NULL,
        [EntregadoPor] int NULL,
        CONSTRAINT [PK_Ordenes] PRIMARY KEY ([OrdenID]),
        CONSTRAINT [FK_Ordenes_ClienteID] FOREIGN KEY ([ClienteID]) REFERENCES [Clientes] ([ClienteID]),
        CONSTRAINT [FK_Ordenes_EntregadoPor] FOREIGN KEY ([EntregadoPor]) REFERENCES [Usuarios] ([UsuarioID]),
        CONSTRAINT [FK_Ordenes_EstadoOrdenID] FOREIGN KEY ([EstadoOrdenID]) REFERENCES [EstadosOrden] ([EstadoOrdenID]),
        CONSTRAINT [FK_Ordenes_RecibidoPor] FOREIGN KEY ([RecibidoPor]) REFERENCES [Usuarios] ([UsuarioID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [CortesCajaDetalle] (
        [CorteDetalleID] int NOT NULL IDENTITY,
        [CorteID] int NOT NULL,
        [MetodoPagoID] int NOT NULL,
        [NumeroTransacciones] int NOT NULL,
        [TotalEsperado] decimal(10,2) NOT NULL,
        [TotalDeclarado] decimal(10,2) NOT NULL,
        [Diferencia] AS ([TotalDeclarado]-[TotalEsperado]) PERSISTED,
        CONSTRAINT [PK_CortesCajaDetalle] PRIMARY KEY ([CorteDetalleID]),
        CONSTRAINT [FK_CortesCajaDetalle_Corte] FOREIGN KEY ([CorteID]) REFERENCES [CortesCaja] ([CorteID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CortesCajaDetalle_Metodo] FOREIGN KEY ([MetodoPagoID]) REFERENCES [MetodosPago] ([MetodoPagoID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [CombosDetalle] (
        [ComboDetalleID] int NOT NULL IDENTITY,
        [ComboID] int NOT NULL,
        [ServicioPrendaID] int NOT NULL,
        [CantidadMinima] int NOT NULL,
        CONSTRAINT [PK_CombosDetalle] PRIMARY KEY ([ComboDetalleID]),
        CONSTRAINT [FK_CombosDetalle_ComboID] FOREIGN KEY ([ComboID]) REFERENCES [Combos] ([ComboID]),
        CONSTRAINT [FK_CombosDetalle_ServicioPrendaID] FOREIGN KEY ([ServicioPrendaID]) REFERENCES [ServiciosPrendas] ([ServicioPrendaID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [HistorialEstadosOrden] (
        [HistorialEstadoID] int NOT NULL IDENTITY,
        [OrdenID] int NOT NULL,
        [EstadoOrdenID] int NOT NULL,
        [FechaCambio] datetime NOT NULL DEFAULT ((getdate())),
        [CambiadoPor] int NOT NULL,
        [Comentarios] varchar(500) NULL,
        CONSTRAINT [PK_HistorialEstadosOrden] PRIMARY KEY ([HistorialEstadoID]),
        CONSTRAINT [FK_HistorialEstadosOrden_CambiadoPor] FOREIGN KEY ([CambiadoPor]) REFERENCES [Usuarios] ([UsuarioID]),
        CONSTRAINT [FK_HistorialEstadosOrden_EstadoOrdenID] FOREIGN KEY ([EstadoOrdenID]) REFERENCES [EstadosOrden] ([EstadoOrdenID]),
        CONSTRAINT [FK_HistorialEstadosOrden_OrdenID] FOREIGN KEY ([OrdenID]) REFERENCES [Ordenes] ([OrdenID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [OrdenesDescuentos] (
        [OrdenDescuentoID] int NOT NULL IDENTITY,
        [OrdenID] int NOT NULL,
        [DescuentoID] int NULL,
        [ComboID] int NULL,
        [MontoDescuento] decimal(10,2) NOT NULL,
        [Justificacion] varchar(255) NULL,
        [AplicadoPor] int NOT NULL,
        CONSTRAINT [PK_OrdenesDescuentos] PRIMARY KEY ([OrdenDescuentoID]),
        CONSTRAINT [FK_OrdenesDescuentos_AplicadoPor] FOREIGN KEY ([AplicadoPor]) REFERENCES [Usuarios] ([UsuarioID]),
        CONSTRAINT [FK_OrdenesDescuentos_ComboID] FOREIGN KEY ([ComboID]) REFERENCES [Combos] ([ComboID]),
        CONSTRAINT [FK_OrdenesDescuentos_DescuentoID] FOREIGN KEY ([DescuentoID]) REFERENCES [Descuentos] ([DescuentoID]),
        CONSTRAINT [FK_OrdenesDescuentos_OrdenID] FOREIGN KEY ([OrdenID]) REFERENCES [Ordenes] ([OrdenID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [OrdenesDetalle] (
        [OrdenDetalleID] int NOT NULL IDENTITY,
        [OrdenID] int NOT NULL,
        [NumeroLinea] int NOT NULL,
        [ServicioID] int NOT NULL,
        [ServicioPrendaID] int NULL,
        [PesoKilos] decimal(6,2) NULL,
        [Cantidad] int NULL,
        [PrecioUnitario] decimal(10,2) NOT NULL,
        [Subtotal] decimal(10,2) NOT NULL,
        [DescuentoLinea] decimal(10,2) NOT NULL,
        [TotalLinea] decimal(10,2) NOT NULL,
        [Observaciones] varchar(500) NULL,
        CONSTRAINT [PK_OrdenesDetalle] PRIMARY KEY ([OrdenDetalleID]),
        CONSTRAINT [FK_OrdenesDetalle_OrdenID] FOREIGN KEY ([OrdenID]) REFERENCES [Ordenes] ([OrdenID]),
        CONSTRAINT [FK_OrdenesDetalle_ServicioID] FOREIGN KEY ([ServicioID]) REFERENCES [Servicios] ([ServicioID]),
        CONSTRAINT [FK_OrdenesDetalle_ServicioPrendaID] FOREIGN KEY ([ServicioPrendaID]) REFERENCES [ServiciosPrendas] ([ServicioPrendaID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [Pagos] (
        [PagoID] int NOT NULL IDENTITY,
        [FolioPago] varchar(30) NOT NULL,
        [OrdenID] int NOT NULL,
        [FechaPago] datetime NOT NULL DEFAULT ((getdate())),
        [MontoPago] decimal(10,2) NOT NULL,
        [RecibioPor] int NOT NULL,
        [Observaciones] varchar(500) NULL,
        CONSTRAINT [PK_Pagos] PRIMARY KEY ([PagoID]),
        CONSTRAINT [FK_Pagos_OrdenID] FOREIGN KEY ([OrdenID]) REFERENCES [Ordenes] ([OrdenID]),
        CONSTRAINT [FK_Pagos_RecibioPor] FOREIGN KEY ([RecibioPor]) REFERENCES [Usuarios] ([UsuarioID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE TABLE [PagosDetalle] (
        [PagoDetalleID] int NOT NULL IDENTITY,
        [PagoID] int NOT NULL,
        [MetodoPagoID] int NOT NULL,
        [MontoPagado] decimal(10,2) NOT NULL,
        [Referencia] varchar(100) NULL,
        CONSTRAINT [PK_PagosDetalle] PRIMARY KEY ([PagoDetalleID]),
        CONSTRAINT [FK_PagosDetalle_MetodoPagoID] FOREIGN KEY ([MetodoPagoID]) REFERENCES [MetodosPago] ([MetodoPagoID]),
        CONSTRAINT [FK_PagosDetalle_PagoID] FOREIGN KEY ([PagoID]) REFERENCES [Pagos] ([PagoID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_AuditoriaGeneral_Tabla_FechaOperacion] ON [AuditoriaGeneral] ([Tabla], [FechaOperacion] DESC);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_AuditoriaGeneral_UsuarioID] ON [AuditoriaGeneral] ([UsuarioID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Categorias_NombreCategoria] ON [Categorias] ([NombreCategoria]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Clientes_RegistradoPor] ON [Clientes] ([RegistradoPor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Clientes_NumeroCliente] ON [Clientes] ([NumeroCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_CombosDetalle_ComboID] ON [CombosDetalle] ([ComboID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_CombosDetalle_ServicioPrendaID] ON [CombosDetalle] ([ServicioPrendaID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_CortesCaja_Cajero] ON [CortesCaja] ([CajeroID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_CortesCaja_FechaCorte] ON [CortesCaja] ([FechaCorte]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_CortesCaja_FechaInicio] ON [CortesCaja] ([FechaInicio], [FechaFin]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_CortesCaja_Folio] ON [CortesCaja] ([FolioCorte]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_CortesCajaDetalle_CorteID] ON [CortesCajaDetalle] ([CorteID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_CortesCajaDetalle_MetodoPagoID] ON [CortesCajaDetalle] ([MetodoPagoID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_EstadosOrden_NombreEstado] ON [EstadosOrden] ([NombreEstado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_HistorialEstadosOrden_CambiadoPor] ON [HistorialEstadosOrden] ([CambiadoPor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_HistorialEstadosOrden_EstadoOrdenID] ON [HistorialEstadosOrden] ([EstadoOrdenID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_HistorialEstadosOrden_OrdenID] ON [HistorialEstadosOrden] ([OrdenID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_HistorialReportes_ConfigReporteID] ON [HistorialReportes] ([ConfigReporteID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_MetodosPago_NombreMetodo] ON [MetodosPago] ([NombreMetodo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Ordenes_ClienteID] ON [Ordenes] ([ClienteID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Ordenes_EntregadoPor] ON [Ordenes] ([EntregadoPor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Ordenes_EstadoOrdenID] ON [Ordenes] ([EstadoOrdenID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Ordenes_FechaRecepcion] ON [Ordenes] ([FechaRecepcion] DESC);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Ordenes_RecibidoPor] ON [Ordenes] ([RecibidoPor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Ordenes_FolioOrden] ON [Ordenes] ([FolioOrden]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_OrdenesDescuentos_AplicadoPor] ON [OrdenesDescuentos] ([AplicadoPor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_OrdenesDescuentos_ComboID] ON [OrdenesDescuentos] ([ComboID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_OrdenesDescuentos_DescuentoID] ON [OrdenesDescuentos] ([DescuentoID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_OrdenesDescuentos_OrdenID] ON [OrdenesDescuentos] ([OrdenID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_OrdenesDetalle_OrdenID] ON [OrdenesDetalle] ([OrdenID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_OrdenesDetalle_ServicioID] ON [OrdenesDetalle] ([ServicioID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_OrdenesDetalle_ServicioPrendaID] ON [OrdenesDetalle] ([ServicioPrendaID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Pagos_OrdenID] ON [Pagos] ([OrdenID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Pagos_RecibioPor] ON [Pagos] ([RecibioPor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Pagos_FolioPago] ON [Pagos] ([FolioPago]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_PagosDetalle_MetodoPagoID] ON [PagosDetalle] ([MetodoPagoID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_PagosDetalle_PagoID] ON [PagosDetalle] ([PagoID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Permisos_Modulo] ON [Permisos] ([Modulo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Permisos_NombrePermiso] ON [Permisos] ([NombrePermiso]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Roles_Activo] ON [Roles] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Roles_NombreRol] ON [Roles] ([NombreRol]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_RolesPermisos_PermisoID] ON [RolesPermisos] ([PermisoID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_RolesPermisos_RolID] ON [RolesPermisos] ([RolID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_RolesPermisos_Rol_Permiso] ON [RolesPermisos] ([RolID], [PermisoID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Servicios_Activo] ON [Servicios] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Servicios_CategoriaID] ON [Servicios] ([CategoriaID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Servicios_TipoCobroServicio] ON [Servicios] ([TipoCobroServicio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Servicios_CodigoServicio] ON [Servicios] ([CodigoServicio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_ServiciosPrendas_ServicioID] ON [ServiciosPrendas] ([ServicioID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_ServiciosPrendas_TipoPrendaID] ON [ServiciosPrendas] ([TipoPrendaID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_ServiciosPrendas_Servicio_Prenda] ON [ServiciosPrendas] ([ServicioID], [TipoPrendaID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_TiposPrenda_NombrePrenda] ON [TiposPrenda] ([NombrePrenda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_Usuarios_Activo] ON [Usuarios] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Usuarios_CreadoPor] ON [Usuarios] ([CreadoPor]) WHERE ([CreadoPor] IS NOT NULL)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Usuarios_Email] ON [Usuarios] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_Usuarios_NombreUsuario] ON [Usuarios] ([NombreUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_UsuariosRoles_RolID] ON [UsuariosRoles] ([RolID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE INDEX [IX_UsuariosRoles_UsuarioID] ON [UsuariosRoles] ([UsuarioID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    CREATE UNIQUE INDEX [UK_UsuariosRoles_Usuario_Rol] ON [UsuariosRoles] ([UsuarioID], [RolID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251229211925_EliminaUbicacionesAgregaCampoTexto', N'8.0.22');
END;
GO

COMMIT;
GO

