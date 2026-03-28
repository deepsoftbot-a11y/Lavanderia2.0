-- Script para marcar la migration como aplicada sin ejecutar cambios
-- Esto sincroniza EF Core con la base de datos que ya fue actualizada manualmente

-- Verificar si la tabla de historial existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- Verificar si la migration ya está registrada
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20251229211925_EliminaUbicacionesAgregaCampoTexto')
BEGIN
    -- Registrar la migration como aplicada
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251229211925_EliminaUbicacionesAgregaCampoTexto', N'8.0.0');

    PRINT 'Migration marcada como aplicada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La migration ya está registrada.';
END
