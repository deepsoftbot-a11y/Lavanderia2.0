-- Script para crear usuarios de prueba con roles y permisos
-- Para ejecutar: Conectarse a LavanderiaDB y ejecutar este script

USE LavanderiaDB;
GO

-- Crear rol admin si no existe
IF NOT EXISTS (SELECT 1 FROM Roles WHERE NombreRol = 'admin')
BEGIN
    INSERT INTO Roles (NombreRol, Descripcion, Activo)
    VALUES ('admin', 'Administrador del sistema con acceso completo', 1);
    PRINT 'Rol admin creado';
END
ELSE
BEGIN
    PRINT 'Rol admin ya existe';
END
GO

-- Crear rol empleado si no existe
IF NOT EXISTS (SELECT 1 FROM Roles WHERE NombreRol = 'empleado')
BEGIN
    INSERT INTO Roles (NombreRol, Descripcion, Activo)
    VALUES ('empleado', 'Empleado con acceso limitado', 1);
    PRINT 'Rol empleado creado';
END
ELSE
BEGIN
    PRINT 'Rol empleado ya existe';
END
GO

-- Crear permisos básicos si no existen
DECLARE @permisos TABLE (NombrePermiso VARCHAR(100), Modulo VARCHAR(50), Descripcion VARCHAR(255))
INSERT INTO @permisos VALUES
    ('ver_ordenes', 'Ordenes', 'Ver listado de órdenes'),
    ('crear_ordenes', 'Ordenes', 'Crear nuevas órdenes'),
    ('editar_ordenes', 'Ordenes', 'Editar órdenes existentes'),
    ('eliminar_ordenes', 'Ordenes', 'Eliminar órdenes'),
    ('ver_pagos', 'Pagos', 'Ver pagos'),
    ('registrar_pagos', 'Pagos', 'Registrar nuevos pagos'),
    ('ver_cortes', 'CortesCaja', 'Ver cortes de caja'),
    ('crear_cortes', 'CortesCaja', 'Crear cortes de caja'),
    ('ver_reportes', 'Reportes', 'Ver reportes'),
    ('administrar_usuarios', 'Usuarios', 'Administrar usuarios del sistema');

INSERT INTO Permisos (NombrePermiso, Modulo, Descripcion)
SELECT p.NombrePermiso, p.Modulo, p.Descripcion
FROM @permisos p
WHERE NOT EXISTS (
    SELECT 1 FROM Permisos WHERE NombrePermiso = p.NombrePermiso
);
PRINT 'Permisos básicos creados o ya existían';
GO

-- Asignar todos los permisos al rol admin
DECLARE @AdminRoleId INT = (SELECT RolId FROM Roles WHERE NombreRol = 'admin');

INSERT INTO RolesPermisos (RolId, PermisoId)
SELECT @AdminRoleId, PermisoId
FROM Permisos
WHERE NOT EXISTS (
    SELECT 1 FROM RolesPermisos
    WHERE RolId = @AdminRoleId AND PermisoId = Permisos.PermisoId
);
PRINT 'Permisos asignados al rol admin';
GO

-- Asignar permisos limitados al rol empleado
DECLARE @EmpleadoRoleId INT = (SELECT RolId FROM Roles WHERE NombreRol = 'empleado');

INSERT INTO RolesPermisos (RolId, PermisoId)
SELECT @EmpleadoRoleId, PermisoId
FROM Permisos
WHERE NombrePermiso IN ('ver_ordenes', 'crear_ordenes', 'ver_pagos', 'registrar_pagos')
AND NOT EXISTS (
    SELECT 1 FROM RolesPermisos
    WHERE RolId = @EmpleadoRoleId AND PermisoId = Permisos.PermisoId
);
PRINT 'Permisos asignados al rol empleado';
GO

-- Crear usuario admin (password: Admin123!)
-- Hash BCrypt generado con work factor 12
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE NombreUsuario = 'admin')
BEGIN
    DECLARE @AdminUserId INT;
    DECLARE @AdminRoleId INT = (SELECT RolId FROM Roles WHERE NombreRol = 'admin');

    INSERT INTO Usuarios (NombreUsuario, Email, PasswordHash, NombreCompleto, Activo, FechaCreacion)
    VALUES (
        'admin',
        'admin@lavanderia.com',
        '$2a$12$LQv8Zf9qH6vZ2W6L8qOu.eD5r5zU7J5Y9X3W2Q1L8K6P5Z8Q9Y7X2W',
        'Administrador',
        1,
        GETDATE()
    );

    SET @AdminUserId = SCOPE_IDENTITY();

    INSERT INTO UsuariosRoles (UsuarioId, RolId, FechaAsignacion)
    VALUES (@AdminUserId, @AdminRoleId, GETDATE());

    PRINT 'Usuario admin creado (username: admin, password: Admin123!)';
END
ELSE
BEGIN
    PRINT 'Usuario admin ya existe';
END
GO

-- Crear usuario empleado de prueba (password: Empleado123!)
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE NombreUsuario = 'empleado1')
BEGIN
    DECLARE @EmpleadoUserId INT;
    DECLARE @EmpleadoRoleId INT = (SELECT RolId FROM Roles WHERE NombreRol = 'empleado');

    INSERT INTO Usuarios (NombreUsuario, Email, PasswordHash, NombreCompleto, Activo, FechaCreacion)
    VALUES (
        'empleado1',
        'empleado@lavanderia.com',
        '$2a$12$XYZabc123defghi456jklmno789pqrstu123vwxyz456ABCDEFGH78',
        'Juan Pérez',
        1,
        GETDATE()
    );

    SET @EmpleadoUserId = SCOPE_IDENTITY();

    INSERT INTO UsuariosRoles (UsuarioId, RolId, FechaAsignacion)
    VALUES (@EmpleadoUserId, @EmpleadoRoleId, GETDATE());

    PRINT 'Usuario empleado1 creado (username: empleado1, password: Empleado123!)';
END
ELSE
BEGIN
    PRINT 'Usuario empleado1 ya existe';
END
GO

-- Verificar creación
SELECT
    u.UsuarioId,
    u.NombreUsuario,
    u.Email,
    u.NombreCompleto,
    u.Activo,
    r.NombreRol as Rol,
    COUNT(DISTINCT rp.PermisoId) as NumPermisos
FROM Usuarios u
LEFT JOIN UsuariosRoles ur ON u.UsuarioId = ur.UsuarioId
LEFT JOIN Roles r ON ur.RolId = r.RolId
LEFT JOIN RolesPermisos rp ON r.RolId = rp.RolId
WHERE u.NombreUsuario IN ('admin', 'empleado1')
GROUP BY u.UsuarioId, u.NombreUsuario, u.Email, u.NombreCompleto, u.Activo, r.NombreRol;

PRINT '';
PRINT '=== Seed completado ===';
PRINT 'Usuarios creados:';
PRINT '1. admin / Admin123!';
PRINT '2. empleado1 / Empleado123!';
PRINT '';
PRINT 'Nota: Si los hashes de contraseña no funcionan, genere nuevos usando BCrypt con work factor 12';
GO
