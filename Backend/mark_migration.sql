-- Script para crear la tabla de historial de migraciones de EF Core en PostgreSQL
-- y registrar la migración inicial como aplicada.
-- Ejecutar solo si la base de datos fue creada manualmente (sin dotnet ef database update).

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId"    varchar(150) NOT NULL,
    "ProductVersion" varchar(32)  NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('InitialPostgres', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
