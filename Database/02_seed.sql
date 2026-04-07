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

INSERT INTO public."Permisos" ("PermisoID", "NombrePermiso", "Modulo", "Seccion", "Etiqueta", "Descripcion") OVERRIDING SYSTEM VALUE VALUES
    -- Dashboard (1)
    (1,  'dashboard.general:view',      'dashboard', 'general',    'Ver dashboard',                    'Acceso a la pantalla principal del dashboard'),

    -- Orders — lista (2)
    (2,  'orders.lista:view',           'orders',    'lista',      'Ver listado de órdenes',           'Ver la tabla de órdenes con filtros'),
    (3,  'orders.lista:export',         'orders',    'lista',      'Exportar órdenes',                 'Exportar el listado de órdenes a archivo'),

    -- Orders — nueva (1)
    (4,  'orders.nueva:create',         'orders',    'nueva',      'Crear nueva orden',                'Acceso a la pantalla de nueva venta y crear órdenes'),

    -- Orders — detalle (3)
    (5,  'orders.detalle:view',         'orders',    'detalle',    'Ver detalle de orden',             'Ver el detalle completo de una orden'),
    (6,  'orders.detalle:edit',         'orders',    'detalle',    'Cambiar estado de orden',          'Modificar el estado de una orden existente'),
    (7,  'orders.detalle:pay',          'orders',    'detalle',    'Agregar pago a orden',             'Registrar pagos sobre una orden'),

    -- Orders — corte (1)
    (8,  'orders.corte:manage',         'orders',    'corte',      'Realizar corte de caja',           'Acceso al modal de corte de caja'),

    -- Services — servicios (4)
    (9,  'services.servicios:view',     'services',  'servicios',  'Ver servicios',                    'Ver el catálogo de servicios'),
    (10, 'services.servicios:create',   'services',  'servicios',  'Crear servicio',                   'Agregar nuevos servicios al catálogo'),
    (11, 'services.servicios:edit',     'services',  'servicios',  'Editar servicio',                  'Modificar servicios existentes'),
    (12, 'services.servicios:delete',   'services',  'servicios',  'Eliminar servicio',                'Eliminar servicios del catálogo'),

    -- Services — categorias (4)
    (13, 'services.categorias:view',    'services',  'categorias', 'Ver categorías',                   'Ver las categorías de servicios'),
    (14, 'services.categorias:create',  'services',  'categorias', 'Crear categoría',                  'Agregar nuevas categorías'),
    (15, 'services.categorias:edit',    'services',  'categorias', 'Editar categoría',                 'Modificar categorías existentes'),
    (16, 'services.categorias:delete',  'services',  'categorias', 'Eliminar categoría',               'Eliminar categorías'),

    -- Services — prendas (4)
    (17, 'services.prendas:view',       'services',  'prendas',    'Ver tipos de prendas',             'Ver el catálogo de tipos de prendas'),
    (18, 'services.prendas:create',     'services',  'prendas',    'Crear tipo de prenda',             'Agregar nuevos tipos de prendas'),
    (19, 'services.prendas:edit',       'services',  'prendas',    'Editar tipo de prenda',            'Modificar tipos de prendas existentes'),
    (20, 'services.prendas:delete',     'services',  'prendas',    'Eliminar tipo de prenda',          'Eliminar tipos de prendas'),

    -- Services — precios (4)
    (21, 'services.precios:view',       'services',  'precios',    'Ver precios por prenda',           'Ver la tabla de precios por prenda y servicio'),
    (22, 'services.precios:create',     'services',  'precios',    'Crear precio',                     'Agregar nuevos precios'),
    (23, 'services.precios:edit',       'services',  'precios',    'Editar precio',                    'Modificar precios existentes'),
    (24, 'services.precios:delete',     'services',  'precios',    'Eliminar precio',                  'Eliminar precios'),

    -- Services — descuentos (4)
    (25, 'services.descuentos:view',    'services',  'descuentos', 'Ver descuentos',                   'Ver el catálogo de descuentos'),
    (26, 'services.descuentos:create',  'services',  'descuentos', 'Crear descuento',                  'Agregar nuevos descuentos'),
    (27, 'services.descuentos:edit',    'services',  'descuentos', 'Editar descuento',                 'Modificar descuentos existentes'),
    (28, 'services.descuentos:delete',  'services',  'descuentos', 'Eliminar descuento',               'Eliminar descuentos'),

    -- Users — usuarios (5)
    (29, 'users.usuarios:view',         'users',     'usuarios',   'Ver listado de usuarios',          'Ver la tabla de usuarios del sistema'),
    (30, 'users.usuarios:create',       'users',     'usuarios',   'Crear usuario',                    'Crear nuevos usuarios'),
    (31, 'users.usuarios:edit',         'users',     'usuarios',   'Editar usuario',                   'Modificar información de usuarios'),
    (32, 'users.usuarios:delete',       'users',     'usuarios',   'Eliminar usuario',                 'Eliminar usuarios del sistema'),
    (33, 'users.usuarios:toggle',       'users',     'usuarios',   'Activar/desactivar usuario',       'Cambiar estado activo/inactivo de un usuario'),

    -- Users — roles (4)
    (34, 'users.roles:view',            'users',     'roles',      'Ver roles',                        'Ver la lista de roles del sistema'),
    (35, 'users.roles:create',          'users',     'roles',      'Crear rol',                        'Crear nuevos roles'),
    (36, 'users.roles:edit',            'users',     'roles',      'Editar rol',                       'Modificar roles y sus permisos asignados'),
    (37, 'users.roles:delete',          'users',     'roles',      'Eliminar rol',                     'Eliminar roles del sistema')
ON CONFLICT ("PermisoID") DO UPDATE SET
    "NombrePermiso" = EXCLUDED."NombrePermiso",
    "Modulo"        = EXCLUDED."Modulo",
    "Seccion"       = EXCLUDED."Seccion",
    "Etiqueta"      = EXCLUDED."Etiqueta",
    "Descripcion"   = EXCLUDED."Descripcion";
SELECT pg_catalog.setval('public."Permisos_PermisoID_seq"', 37, true);

-- Admin: todos los permisos (1-37)
INSERT INTO public."RolesPermisos" ("RolID", "PermisoID") OVERRIDING SYSTEM VALUE
SELECT 1, generate_series(1, 37)
ON CONFLICT DO NOTHING;

-- Empleado: permisos operativos básicos
INSERT INTO public."RolesPermisos" ("RolID", "PermisoID") OVERRIDING SYSTEM VALUE VALUES
    (2, 2),   -- orders.lista:view
    (2, 3),   -- orders.lista:export
    (2, 4),   -- orders.nueva:create
    (2, 5),   -- orders.detalle:view
    (2, 6),   -- orders.detalle:edit
    (2, 7),   -- orders.detalle:pay
    (2, 8),   -- orders.corte:manage
    (2, 9),   -- services.servicios:view
    (2, 13),  -- services.categorias:view
    (2, 17),  -- services.prendas:view
    (2, 21),  -- services.precios:view
    (2, 25)   -- services.descuentos:view
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
