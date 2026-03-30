-- =============================================================================
-- LavanderiaDB - Datos base del sistema
-- Ejecutar después de 01_schema.sql
-- =============================================================================
-- Usuarios:
--   admin     / Admin123!
--   empleado1 / Empleado123!
-- =============================================================================

SET client_encoding = 'UTF8';
SELECT pg_catalog.set_config('search_path', '', false);

-- =============================================================================
-- CATÁLOGOS
-- =============================================================================

INSERT INTO public."EstadosOrden" ("EstadoOrdenID", "NombreEstado", "ColorEstado", "OrdenProceso") OVERRIDING SYSTEM VALUE VALUES
    (1, 'Recibida',    '#3B82F6', 1),
    (2, 'En Proceso',  '#F59E0B', 2),
    (3, 'Lista',       '#10B981', 3),
    (4, 'Entregada',   '#6B7280', 4),
    (5, 'Cancelada',   '#EF4444', 5)
ON CONFLICT ("EstadoOrdenID") DO NOTHING;
SELECT pg_catalog.setval('public."EstadosOrden_EstadoOrdenID_seq"', 5, true);

INSERT INTO public."MetodosPago" ("MetodoPagoID", "NombreMetodo", "RequiereReferencia", "Activo") OVERRIDING SYSTEM VALUE VALUES
    (1, 'Efectivo',      false, true),
    (2, 'Tarjeta',       true,  true),
    (3, 'Transferencia', true,  true),
    (4, 'Otros',         false, true)
ON CONFLICT ("MetodoPagoID") DO NOTHING;
SELECT pg_catalog.setval('public."MetodosPago_MetodoPagoID_seq"', 4, true);

INSERT INTO public."Categorias" ("CategoriaID", "NombreCategoria", "Descripcion", "Activo") OVERRIDING SYSTEM VALUE VALUES
    (1, 'Lavandería',    'Servicios de lavado de ropa en general',              true),
    (2, 'Tintorería',    'Limpieza en seco de prendas delicadas',               true),
    (3, 'Planchado',     'Servicio de planchado y doblado',                     true),
    (4, 'Calzado',       'Limpieza y restauración de calzado',                  true),
    (5, 'Ropa de Cama',  'Sábanas, cobijas, edredones y almohadas',             true),
    (6, 'Especialidades','Prendas con tratamiento especial o delicadas',        true)
ON CONFLICT ("CategoriaID") DO NOTHING;
SELECT pg_catalog.setval('public."Categorias_CategoriaID_seq"', 6, true);

INSERT INTO public."TiposPrenda" ("TipoPrendaID", "NombrePrenda", "Descripcion", "Activo") OVERRIDING SYSTEM VALUE VALUES
    (1,  'Camisa',              'Camisa de vestir o casual',                    true),
    (2,  'Pantalón',            'Pantalón de vestir o casual',                  true),
    (3,  'Vestido',             'Vestido de dama',                              true),
    (4,  'Falda',               'Falda de dama',                                true),
    (5,  'Saco / Blazer',       'Saco o blazer formal',                         true),
    (6,  'Corbata',             'Corbata o lazo',                               true),
    (7,  'Traje completo',      'Saco y pantalón de traje',                     true),
    (8,  'Abrigo / Gabardina',  'Prenda de abrigo exterior',                    true),
    (9,  'Suéter / Jersey',     'Prenda de tejido de punto',                    true),
    (10, 'Jeans',               'Pantalón de mezclilla',                        true),
    (11, 'Ropa deportiva',      'Pants, short, playera deportiva',              true),
    (12, 'Sábana individual',   'Sábana de cama individual',                    true),
    (13, 'Sábana matrimonial',  'Sábana de cama matrimonial o queen',           true),
    (14, 'Funda de almohada',   'Funda de almohada',                            true),
    (15, 'Cobija / Manta',      'Cobija o manta de cama',                       true),
    (16, 'Edredón',             'Edredón o quilt',                              true),
    (17, 'Toalla',              'Toalla de baño o mano',                        true),
    (18, 'Alfombra pequeña',    'Alfombra o tapete pequeño (hasta 1.5 m²)',     true),
    (19, 'Alfombra mediana',    'Alfombra mediana (1.5 – 4 m²)',                true),
    (20, 'Alfombra grande',     'Alfombra grande (más de 4 m²)',                true)
ON CONFLICT ("TipoPrendaID") DO NOTHING;
SELECT pg_catalog.setval('public."TiposPrenda_TipoPrendaID_seq"', 20, true);

-- =============================================================================
-- ROLES Y PERMISOS
-- =============================================================================

INSERT INTO public."Roles" ("RolID", "NombreRol", "Descripcion", "Activo") OVERRIDING SYSTEM VALUE VALUES
    (1, 'admin',    'Administrador del sistema con acceso completo', true),
    (2, 'empleado', 'Empleado con acceso limitado',                  true)
ON CONFLICT ("RolID") DO NOTHING;
SELECT pg_catalog.setval('public."Roles_RolID_seq"', 2, true);

INSERT INTO public."Permisos" ("PermisoID", "NombrePermiso", "Modulo", "Descripcion") OVERRIDING SYSTEM VALUE VALUES
    -- Usuarios
    (1,  'Crear_Usuario',        'Usuarios',       'Crear nuevos usuarios del sistema'),
    (2,  'Modificar_Usuario',    'Usuarios',       'Modificar información de usuarios'),
    (3,  'Eliminar_Usuario',     'Usuarios',       'Desactivar usuarios'),
    (4,  'Ver_Usuarios',         'Usuarios',       'Consultar lista de usuarios'),
    (5,  'Asignar_Roles',        'Usuarios',       'Asignar roles a usuarios'),
    -- Clientes
    (6,  'Crear_Cliente',        'Clientes',       'Registrar nuevos clientes'),
    (7,  'Modificar_Cliente',    'Clientes',       'Modificar información de clientes'),
    (8,  'Ver_Clientes',         'Clientes',       'Consultar clientes'),
    (9,  'Gestionar_Credito',    'Clientes',       'Configurar límites de crédito'),
    -- Órdenes
    (10, 'Crear_Orden',          'Ordenes',        'Crear nuevas órdenes de servicio'),
    (11, 'Modificar_Orden',      'Ordenes',        'Modificar órdenes'),
    (12, 'Cancelar_Orden',       'Ordenes',        'Cancelar órdenes'),
    (13, 'Ver_Ordenes',          'Ordenes',        'Consultar órdenes'),
    (14, 'Cambiar_Estado_Orden', 'Ordenes',        'Cambiar estado de órdenes'),
    (15, 'Entregar_Orden',       'Ordenes',        'Marcar orden como entregada'),
    -- Pagos
    (16, 'Registrar_Pago',       'Pagos',          'Registrar pagos de clientes'),
    (17, 'Cancelar_Pago',        'Pagos',          'Cancelar pagos'),
    (18, 'Ver_Pagos',            'Pagos',          'Consultar pagos'),
    (19, 'Ver_Saldo_Cliente',    'Pagos',          'Ver saldo de cliente'),
    -- Servicios
    (20, 'Crear_Servicio',       'Servicios',      'Crear nuevos servicios'),
    (21, 'Modificar_Servicio',   'Servicios',      'Modificar servicios'),
    (22, 'Modificar_Precios',    'Servicios',      'Modificar precios de servicios'),
    (23, 'Ver_Servicios',        'Servicios',      'Consultar servicios'),
    -- Descuentos
    (24, 'Aplicar_Descuento',    'Descuentos',     'Aplicar descuentos a órdenes'),
    (25, 'Crear_Combo',          'Descuentos',     'Crear combos promocionales'),
    (26, 'Modificar_Combo',      'Descuentos',     'Modificar combos'),
    -- Reportes
    (27, 'Generar_Reporte',      'Reportes',       'Generar reportes del sistema'),
    (28, 'Configurar_Reporte',   'Reportes',       'Configurar reportes automáticos'),
    (29, 'Ver_Dashboard',        'Reportes',       'Ver tablero de control'),
    -- Configuración
    (30, 'Configurar_Sistema',   'Configuracion',  'Configurar parámetros del sistema'),
    (31, 'Ver_Auditoria',        'Configuracion',  'Ver log de auditoría'),
    (32, 'Gestionar_Ubicaciones','Configuracion',  'Gestionar ubicaciones de almacén')
ON CONFLICT ("PermisoID") DO NOTHING;
SELECT pg_catalog.setval('public."Permisos_PermisoID_seq"', 32, true);

-- Admin: todos los permisos (1-32)
INSERT INTO public."RolesPermisos" ("RolID", "PermisoID") OVERRIDING SYSTEM VALUE
SELECT 1, generate_series(1, 32)
ON CONFLICT DO NOTHING;

-- Empleado: permisos operativos básicos
INSERT INTO public."RolesPermisos" ("RolID", "PermisoID") OVERRIDING SYSTEM VALUE VALUES
    (2, 8),   -- Ver_Clientes
    (2, 10),  -- Crear_Orden
    (2, 13),  -- Ver_Ordenes
    (2, 14),  -- Cambiar_Estado_Orden
    (2, 15),  -- Entregar_Orden
    (2, 16),  -- Registrar_Pago
    (2, 18),  -- Ver_Pagos
    (2, 19),  -- Ver_Saldo_Cliente
    (2, 23),  -- Ver_Servicios
    (2, 29)   -- Ver_Dashboard
ON CONFLICT DO NOTHING;

SELECT pg_catalog.setval('public."RolesPermisos_RolPermisoID_seq"', 42, true);

-- =============================================================================
-- USUARIOS BASE
-- Contraseñas hasheadas con BCrypt work factor 12:
--   admin     -> Admin123!
--   empleado1 -> Empleado123!
-- =============================================================================

INSERT INTO public."Usuarios" ("UsuarioID", "NombreUsuario", "Email", "PasswordHash", "NombreCompleto", "Activo", "FechaCreacion") OVERRIDING SYSTEM VALUE VALUES
    (1, 'admin',     'admin@lavanderia.com',    '$2a$12$2HTmxMHPxO56hfqxQihzauORsfkNfO4SOYYYXgx/eGRSQsq23aVAu', 'Administrador', true, '2026-01-01 00:00:00+00'),
    (2, 'empleado1', 'empleado@lavanderia.com', '$2a$12$jRnHOveRxECX/sWlISJsxeDj6ahRMq07iNCbuKS0almTRQApo2eyS', 'Juan Pérez',    true, '2026-01-01 00:00:00+00')
ON CONFLICT ("UsuarioID") DO NOTHING;
SELECT pg_catalog.setval('public."Usuarios_UsuarioID_seq"', 2, true);

INSERT INTO public."UsuariosRoles" ("UsuarioID", "RolID") OVERRIDING SYSTEM VALUE VALUES
    (1, 1),  -- admin -> admin
    (2, 2)   -- empleado1 -> empleado
ON CONFLICT DO NOTHING;
SELECT pg_catalog.setval('public."UsuariosRoles_UsuarioRolID_seq"', 2, true);

-- =============================================================================
-- CONFIGURACIÓN DE REPORTES
-- =============================================================================

INSERT INTO public."ConfiguracionReportes" ("ConfigReporteID", "NombreReporte", "TipoReporte", "Frecuencia", "FormatoExportacion", "DestinatariosEmail", "HoraEnvio", "Activo", "ParametrosJSON") OVERRIDING SYSTEM VALUE VALUES
    (1, 'Reporte de corte de caja', 'CorteCaja', 'EventoCorte', 'EXCEL', 'info@deepsoft.mx', NULL, true, NULL)
ON CONFLICT ("ConfigReporteID") DO NOTHING;
SELECT pg_catalog.setval('public."ConfiguracionReportes_ConfigReporteID_seq"', 1, true);
