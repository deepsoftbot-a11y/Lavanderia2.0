-- =============================================================
-- SEED v2: Permisos granulares — Lavanderia 2.0
-- Formato: modulo.seccion:accion
-- Nuevas columnas: Seccion, Etiqueta
--
-- Ejecutar:
--   docker exec -i lavanderia_postgres psql -U lavanderia_user -d LavanderiaDB < seed_permissions_v2.sql
--
-- ADVERTENCIA: Este script elimina todos los permisos y asignaciones
-- de roles existentes y los reemplaza con el nuevo catálogo.
-- =============================================================

DO $$
DECLARE
    v_admin_rol_id     INT;
    v_empleado_rol_id  INT;
BEGIN

-- =============================================================
-- 1. LIMPIAR PERMISOS Y ASIGNACIONES ANTERIORES
-- =============================================================
DELETE FROM "RolesPermisos";
DELETE FROM "Permisos";
PERFORM setval('"Permisos_PermisoID_seq"', 1, false);

-- =============================================================
-- 2. INSERTAR CATÁLOGO NUEVO (37 permisos)
-- =============================================================
INSERT INTO "Permisos" ("PermisoID", "NombrePermiso", "Modulo", "Seccion", "Etiqueta", "Descripcion")
OVERRIDING SYSTEM VALUE VALUES

-- DASHBOARD (1)
(1,  'dashboard.general:view',      'dashboard', 'general',    'Ver dashboard',                    'Acceso a la pantalla principal del dashboard'),

-- ORDERS (7)
(2,  'orders.lista:view',           'orders',    'lista',      'Ver listado de órdenes',           'Ver la tabla de órdenes con filtros'),
(3,  'orders.lista:export',         'orders',    'lista',      'Exportar órdenes',                 'Exportar el listado de órdenes a archivo'),
(4,  'orders.nueva:create',         'orders',    'nueva',      'Crear nueva orden',                'Acceso a la pantalla de nueva venta y crear órdenes'),
(5,  'orders.detalle:view',         'orders',    'detalle',    'Ver detalle de orden',             'Ver el detalle completo de una orden'),
(6,  'orders.detalle:edit',         'orders',    'detalle',    'Cambiar estado de orden',          'Modificar el estado de una orden existente'),
(7,  'orders.detalle:pay',          'orders',    'detalle',    'Agregar pago a orden',             'Registrar pagos sobre una orden'),
(8,  'orders.corte:manage',         'orders',    'corte',      'Realizar corte de caja',           'Acceso al modal de corte de caja'),

-- SERVICES — servicios (4)
(9,  'services.servicios:view',     'services',  'servicios',  'Ver servicios',                    'Ver el catálogo de servicios'),
(10, 'services.servicios:create',   'services',  'servicios',  'Crear servicio',                   'Agregar nuevos servicios al catálogo'),
(11, 'services.servicios:edit',     'services',  'servicios',  'Editar servicio',                  'Modificar servicios existentes'),
(12, 'services.servicios:delete',   'services',  'servicios',  'Eliminar servicio',                'Eliminar servicios del catálogo'),

-- SERVICES — categorias (4)
(13, 'services.categorias:view',    'services',  'categorias', 'Ver categorías',                   'Ver las categorías de servicios'),
(14, 'services.categorias:create',  'services',  'categorias', 'Crear categoría',                  'Agregar nuevas categorías'),
(15, 'services.categorias:edit',    'services',  'categorias', 'Editar categoría',                 'Modificar categorías existentes'),
(16, 'services.categorias:delete',  'services',  'categorias', 'Eliminar categoría',               'Eliminar categorías'),

-- SERVICES — prendas (4)
(17, 'services.prendas:view',       'services',  'prendas',    'Ver tipos de prendas',             'Ver el catálogo de tipos de prendas'),
(18, 'services.prendas:create',     'services',  'prendas',    'Crear tipo de prenda',             'Agregar nuevos tipos de prendas'),
(19, 'services.prendas:edit',       'services',  'prendas',    'Editar tipo de prenda',            'Modificar tipos de prendas existentes'),
(20, 'services.prendas:delete',     'services',  'prendas',    'Eliminar tipo de prenda',          'Eliminar tipos de prendas'),

-- SERVICES — precios (4)
(21, 'services.precios:view',       'services',  'precios',    'Ver precios por prenda',           'Ver la tabla de precios por prenda y servicio'),
(22, 'services.precios:create',     'services',  'precios',    'Crear precio',                     'Agregar nuevos precios'),
(23, 'services.precios:edit',       'services',  'precios',    'Editar precio',                    'Modificar precios existentes'),
(24, 'services.precios:delete',     'services',  'precios',    'Eliminar precio',                  'Eliminar precios'),

-- SERVICES — descuentos (4)
(25, 'services.descuentos:view',    'services',  'descuentos', 'Ver descuentos',                   'Ver el catálogo de descuentos'),
(26, 'services.descuentos:create',  'services',  'descuentos', 'Crear descuento',                  'Agregar nuevos descuentos'),
(27, 'services.descuentos:edit',    'services',  'descuentos', 'Editar descuento',                 'Modificar descuentos existentes'),
(28, 'services.descuentos:delete',  'services',  'descuentos', 'Eliminar descuento',               'Eliminar descuentos'),

-- USERS — usuarios (5)
(29, 'users.usuarios:view',         'users',     'usuarios',   'Ver listado de usuarios',          'Ver la tabla de usuarios del sistema'),
(30, 'users.usuarios:create',       'users',     'usuarios',   'Crear usuario',                    'Crear nuevos usuarios'),
(31, 'users.usuarios:edit',         'users',     'usuarios',   'Editar usuario',                   'Modificar información de usuarios'),
(32, 'users.usuarios:delete',       'users',     'usuarios',   'Eliminar usuario',                 'Eliminar usuarios del sistema'),
(33, 'users.usuarios:toggle',       'users',     'usuarios',   'Activar/desactivar usuario',       'Cambiar estado activo/inactivo de un usuario'),

-- USERS — roles (4)
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

PERFORM setval('"Permisos_PermisoID_seq"', 37);

-- =============================================================
-- 3. ASIGNAR PERMISOS A ROLES
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
    'orders.lista:view',
    'orders.lista:export',
    'orders.nueva:create',
    'orders.detalle:view',
    'orders.detalle:edit',
    'orders.detalle:pay',
    'orders.corte:manage',
    'services.servicios:view',
    'services.categorias:view',
    'services.prendas:view',
    'services.precios:view',
    'services.descuentos:view'
)
ON CONFLICT ("RolID", "PermisoID") DO NOTHING;

RAISE NOTICE 'Permisos v2 aplicados correctamente. Total: %', (SELECT COUNT(*) FROM "Permisos");

END $$;

-- =============================================================
-- VERIFICACIÓN
-- =============================================================
SELECT "Modulo", "Seccion", COUNT(*) AS total
FROM "Permisos"
GROUP BY "Modulo", "Seccion"
ORDER BY "Modulo", "Seccion";

SELECT r."NombreRol", COUNT(rp."PermisoID") AS permisos_asignados
FROM "Roles" r
LEFT JOIN "RolesPermisos" rp ON r."RolID" = rp."RolID"
GROUP BY r."NombreRol";
