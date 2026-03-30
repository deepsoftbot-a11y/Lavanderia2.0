-- Script para marcar la migración como aplicada sin ejecutar cambios.
-- Útil para sincronizar EF Core con una base de datos ya actualizada manualmente.

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId"    varchar(150) NOT NULL,
    "ProductVersion" varchar(32)  NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('InitialPostgres', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;

SELECT
    CASE
        WHEN EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = 'InitialPostgres'
        )
        THEN 'Migración registrada correctamente.'
        ELSE 'ERROR: No se pudo registrar la migración.'
    END AS "Estado";
