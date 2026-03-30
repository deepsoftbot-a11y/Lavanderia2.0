-- =============================================================
-- SEED: Información base del sistema — Lavanderia 2.0
-- Ejecutar en una base de datos recién migrada:
--   docker exec -i lavanderia_postgres psql -U lavanderia_user -d LavanderiaDB < seed_admin_user.sql
-- =============================================================

DO $$
DECLARE
    v_admin_rol_id     INT;
    v_empleado_rol_id  INT;
    v_admin_user_id    INT;
BEGIN

-- =============================================================
-- 1. ESTADOS DE ORDEN
-- =============================================================
INSERT INTO "EstadosOrden" ("EstadoOrdenID", "NombreEstado", "ColorEstado", "OrdenProceso")
OVERRIDING SYSTEM VALUE VALUES
    (1, 'Recibida',    '#3B82F6', 1),
    (2, 'En Proceso',  '#F59E0B', 2),
    (3, 'Lista',       '#10B981', 3),
    (4, 'Entregada',   '#6B7280', 4),
    (5, 'Cancelada',   '#EF4444', 5)
ON CONFLICT ("EstadoOrdenID") DO NOTHING;
PERFORM setval('"EstadosOrden_EstadoOrdenID_seq"', 5);

-- =============================================================
-- 2. MÉTODOS DE PAGO
-- =============================================================
INSERT INTO "MetodosPago" ("MetodoPagoID", "NombreMetodo", "RequiereReferencia", "Activo")
OVERRIDING SYSTEM VALUE VALUES
    (1, 'Efectivo',       false, true),
    (2, 'Tarjeta',        true,  true),
    (3, 'Transferencia',  true,  true),
    (4, 'Otros',          false, true)
ON CONFLICT ("MetodoPagoID") DO NOTHING;
PERFORM setval('"MetodosPago_MetodoPagoID_seq"', 4);

-- =============================================================
-- 3. CATEGORÍAS DE SERVICIOS
-- =============================================================
INSERT INTO "Categorias" ("CategoriaID", "NombreCategoria", "Descripcion", "Activo")
OVERRIDING SYSTEM VALUE VALUES
    (1, 'Lavandería',          'Servicios de lavado de ropa en general',         true),
    (2, 'Tintorería',          'Limpieza en seco de prendas delicadas',           true),
    (3, 'Planchado',           'Servicio de planchado y doblado',                 true),
    (4, 'Alfombras y Tapetes', 'Lavado especializado de alfombras y tapetes',     true),
    (5, 'Ropa de Cama',        'Sábanas, cobijas, edredones y almohadas',         true),
    (6, 'Especialidades',      'Prendas con tratamiento especial o delicadas',    true)
ON CONFLICT ("CategoriaID") DO NOTHING;
PERFORM setval('"Categorias_CategoriaID_seq"', 6);

-- =============================================================
-- 4. TIPOS DE PRENDA
-- =============================================================
INSERT INTO "TiposPrenda" ("TipoPrendaID", "NombrePrenda", "Descripcion", "Activo")
OVERRIDING SYSTEM VALUE VALUES
    (1,  'Camisa',           'Camisa de vestir o casual',                  true),
    (2,  'Pantalón',         'Pantalón de vestir o casual',                true),
    (3,  'Vestido',          'Vestido de dama',                            true),
    (4,  'Falda',            'Falda de dama',                              true),
    (5,  'Saco / Blazer',    'Saco o blazer formal',                       true),
    (6,  'Corbata',          'Corbata o lazo',                             true),
    (7,  'Traje completo',   'Saco y pantalón de traje',                   true),
    (8,  'Abrigo / Gabardina','Prenda de abrigo exterior',                 true),
    (9,  'Suéter / Jersey',  'Prenda de tejido de punto',                  true),
    (10, 'Jeans',            'Pantalón de mezclilla',                      true),
    (11, 'Ropa deportiva',   'Pants, short, playera deportiva',            true),
    (12, 'Sábana individual','Sábana de cama individual',                  true),
    (13, 'Sábana matrimonial','Sábana de cama matrimonial o queen',        true),
    (14, 'Funda de almohada','Funda de almohada',                          true),
    (15, 'Cobija / Manta',   'Cobija o manta de cama',                     true),
    (16, 'Edredón',          'Edredón o quilt',                            true),
    (17, 'Toalla',           'Toalla de baño o mano',                      true),
    (18, 'Alfombra pequeña', 'Alfombra o tapete pequeño (hasta 1.5 m²)',   true),
    (19, 'Alfombra mediana', 'Alfombra mediana (1.5 – 4 m²)',              true),
    (20, 'Alfombra grande',  'Alfombra grande (más de 4 m²)',              true)
ON CONFLICT ("TipoPrendaID") DO NOTHING;
PERFORM setval('"TiposPrenda_TipoPrendaID_seq"', 20);

-- =============================================================
-- 5. ROLES
-- =============================================================
INSERT INTO "Roles" ("NombreRol", "Descripcion", "Activo") VALUES
    ('admin',    'Administrador del sistema con acceso completo', true),
    ('empleado', 'Empleado con acceso operativo',                 true)
ON CONFLICT ("NombreRol") DO NOTHING;

-- =============================================================
-- 6. PERMISOS
-- =============================================================
INSERT INTO "Permisos" ("PermisoID", "NombrePermiso", "Modulo", "Descripcion")
OVERRIDING SYSTEM VALUE VALUES
    (1,  'Crear_Usuario',         'Usuarios',      'Crear nuevos usuarios del sistema'),
    (2,  'Modificar_Usuario',     'Usuarios',      'Modificar información de usuarios'),
    (3,  'Eliminar_Usuario',      'Usuarios',      'Desactivar usuarios'),
    (4,  'Ver_Usuarios',          'Usuarios',      'Consultar lista de usuarios'),
    (5,  'Asignar_Roles',         'Usuarios',      'Asignar roles a usuarios'),
    (6,  'Crear_Cliente',         'Clientes',      'Registrar nuevos clientes'),
    (7,  'Modificar_Cliente',     'Clientes',      'Modificar información de clientes'),
    (8,  'Ver_Clientes',          'Clientes',      'Consultar clientes'),
    (9,  'Gestionar_Credito',     'Clientes',      'Configurar límites de crédito'),
    (10, 'Crear_Orden',           'Ordenes',       'Crear nuevas órdenes de servicio'),
    (11, 'Modificar_Orden',       'Ordenes',       'Modificar órdenes'),
    (12, 'Cancelar_Orden',        'Ordenes',       'Cancelar órdenes'),
    (13, 'Ver_Ordenes',           'Ordenes',       'Consultar órdenes'),
    (14, 'Cambiar_Estado_Orden',  'Ordenes',       'Cambiar estado de órdenes'),
    (15, 'Entregar_Orden',        'Ordenes',       'Marcar orden como entregada'),
    (16, 'Registrar_Pago',        'Pagos',         'Registrar pagos de clientes'),
    (17, 'Cancelar_Pago',         'Pagos',         'Cancelar pagos'),
    (18, 'Ver_Pagos',             'Pagos',         'Consultar pagos'),
    (19, 'Ver_Saldo_Cliente',     'Pagos',         'Ver saldo de cliente'),
    (20, 'Crear_Servicio',        'Servicios',     'Crear nuevos servicios'),
    (21, 'Modificar_Servicio',    'Servicios',     'Modificar servicios'),
    (22, 'Modificar_Precios',     'Servicios',     'Modificar precios de servicios'),
    (23, 'Ver_Servicios',         'Servicios',     'Consultar servicios'),
    (24, 'Aplicar_Descuento',     'Descuentos',    'Aplicar descuentos a órdenes'),
    (25, 'Crear_Combo',           'Descuentos',    'Crear combos promocionales'),
    (26, 'Modificar_Combo',       'Descuentos',    'Modificar combos'),
    (27, 'Generar_Reporte',       'Reportes',      'Generar reportes del sistema'),
    (28, 'Configurar_Reporte',    'Reportes',      'Configurar reportes automáticos'),
    (29, 'Ver_Dashboard',         'Reportes',      'Ver tablero de control'),
    (30, 'Configurar_Sistema',    'Configuracion', 'Configurar parámetros del sistema'),
    (31, 'Ver_Auditoria',         'Configuracion', 'Ver log de auditoría'),
    (32, 'Gestionar_Ubicaciones', 'Configuracion', 'Gestionar ubicaciones de almacén')
ON CONFLICT ("PermisoID") DO NOTHING;
PERFORM setval('"Permisos_PermisoID_seq"', 32);

-- =============================================================
-- 7. ROLES → PERMISOS
-- =============================================================
SELECT "RolID" INTO v_admin_rol_id    FROM "Roles" WHERE "NombreRol" = 'admin';
SELECT "RolID" INTO v_empleado_rol_id FROM "Roles" WHERE "NombreRol" = 'empleado';

-- Admin: todos los permisos
INSERT INTO "RolesPermisos" ("RolID", "PermisoID")
SELECT v_admin_rol_id, "PermisoID" FROM "Permisos"
ON CONFLICT ("RolID", "PermisoID") DO NOTHING;

-- Empleado: permisos operativos
INSERT INTO "RolesPermisos" ("RolID", "PermisoID")
SELECT v_empleado_rol_id, "PermisoID"
FROM "Permisos"
WHERE "NombrePermiso" IN (
    'Ver_Ordenes', 'Crear_Orden', 'Cambiar_Estado_Orden', 'Entregar_Orden',
    'Ver_Pagos', 'Registrar_Pago', 'Ver_Saldo_Cliente',
    'Ver_Clientes', 'Ver_Servicios', 'Ver_Dashboard'
)
ON CONFLICT ("RolID", "PermisoID") DO NOTHING;

-- =============================================================
-- 8. USUARIO ADMINISTRADOR
--    username: admin  |  password: Admin123!
--    Hash BCrypt work factor 12
-- =============================================================
INSERT INTO "Usuarios" ("NombreUsuario", "Email", "PasswordHash", "NombreCompleto", "Activo")
VALUES (
    'admin',
    'admin@lavanderia.com',
    '$2a$12$2HTmxMHPxO56hfqxQihzauORsfkNfO4SOYYYXgx/eGRSQsq23aVAu',
    'Administrador',
    true
)
ON CONFLICT ("NombreUsuario") DO NOTHING
RETURNING "UsuarioID" INTO v_admin_user_id;

IF v_admin_user_id IS NOT NULL THEN
    INSERT INTO "UsuariosRoles" ("UsuarioID", "RolID")
    VALUES (v_admin_user_id, v_admin_rol_id)
    ON CONFLICT ("UsuarioID", "RolID") DO NOTHING;
    RAISE NOTICE 'Usuario admin creado correctamente.';
ELSE
    RAISE NOTICE 'Usuario admin ya existía, sin cambios.';
END IF;

END $$;

-- =============================================================
-- VERIFICACIÓN
-- =============================================================
SELECT 'EstadosOrden'  AS tabla, COUNT(*) AS registros FROM "EstadosOrden"
UNION ALL SELECT 'MetodosPago',   COUNT(*) FROM "MetodosPago"
UNION ALL SELECT 'Categorias',    COUNT(*) FROM "Categorias"
UNION ALL SELECT 'TiposPrenda',   COUNT(*) FROM "TiposPrenda"
UNION ALL SELECT 'Roles',         COUNT(*) FROM "Roles"
UNION ALL SELECT 'Permisos',      COUNT(*) FROM "Permisos"
UNION ALL SELECT 'Usuarios',      COUNT(*) FROM "Usuarios";
