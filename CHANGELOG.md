# Registro de cambios

Todos los cambios notables de **Lavanderia 2.0** se documentan en este archivo.

Formato basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

---

## [Sin publicar] — 2026-03-29

### Seguridad

- **Se eliminó el secreto JWT hardcodeado** de `appsettings.json`. El campo `SecretKey` ahora se deja vacío en los archivos de configuración y debe proporcionarse mediante variable de entorno o dotnet user-secrets en tiempo de ejecución. La API lanza una excepción clara al iniciar si el secreto está ausente o tiene menos de 32 caracteres.
- **Se eliminó la contraseña SMTP hardcodeada** de `appsettings.Development.json`. La contraseña real de la aplicación Gmail estaba guardada en texto plano y fue reemplazada por un marcador vacío. Las credenciales ahora deben configurarse con dotnet user-secrets.
- **Se restringió la política CORS** de `AllowAnyOrigin()` a una lista de orígenes explícita leída desde la configuración (`AllowedOrigins`). Por defecto permite `localhost:5173` y `localhost:3000` en desarrollo.
- **Se agregó limitación de peticiones** en el endpoint de inicio de sesión — ventana fija de 5 solicitudes por minuto por IP. Devuelve HTTP 429 al exceder el límite, previniendo ataques de fuerza bruta.
- **Se eliminaron los logs de consola en producción** del cliente Axios del Frontend. Todas las llamadas a `console.log` / `console.warn` ahora están protegidas por `import.meta.env.DEV`, por lo que son eliminadas automáticamente en los bundles de producción.

### Mejoras

- **Se redujo la duración del token JWT** de 480 minutos (8 h) a 60 minutos en producción, y de 1 440 minutos (24 h) a 120 minutos en desarrollo, reduciendo la ventana de exposición ante tokens robados.
- **Se agregó verificación local de expiración del JWT** en `authStore`. Antes de realizar cualquier llamada autenticada a la API, el token almacenado se decodifica localmente (vía `atob`) y se descarta de inmediato si está vencido, evitando una petición innecesaria al servidor y una confusa respuesta 401.
- **Se redujo la agresividad de reintentos de EF Core**: `maxRetryCount` bajó de 5 → 3 y `maxRetryDelay` de 30 s → 10 s, evitando demoras en cascada cuando la base de datos no está disponible.
- **Se deshabilitó el logging sensible de SQL en producción**: `EnableSensitiveDataLogging()` y `EnableDetailedErrors()` ahora solo se activan cuando la aplicación corre en entorno `Development`.
- **Se agregó `.env.production.example`** al Frontend, documentando todas las variables de entorno necesarias para un despliegue en producción (incluyendo el requisito de HTTPS para la URL de la API).
- **Se actualizó `.env.example`** para incluir `VITE_AUTH_MODE`, `VITE_APP_NAME` y `VITE_APP_VERSION` junto a las variables existentes.

---

## [0.1.0] — 2026-03-28 — Lanzamiento inicial

### Agregado

- Sistema completo de gestión de lavandería y tintorería (Lavanderia 2.0).
- **Backend**: API REST con ASP.NET Core 8 siguiendo Arquitectura Limpia + DDD + CQRS.
  - Capas: Domain, Application (MediatR + FluentValidation + AutoMapper), Infrastructure (EF Core + Dapper + procedimientos almacenados), API (autenticación JWT, manejador global de excepciones, Serilog).
  - Base de datos SQL Server (`LavanderiaDB`) con migraciones de EF Core.
- **Frontend**: SPA con React 19 + TypeScript.
  - Módulos verticales por funcionalidad: autenticación, órdenes, clientes, servicios, usuarios, dashboard.
  - Zustand + Immer para estado global; instancia centralizada de Axios con interceptores.
- Dominios de negocio: Órdenes (folios, ciclo de vida, descuentos), Clientes (CRM), Servicios (catálogo y precios), Pagos, Cortes de Caja, Usuarios/Roles/Permisos.
- Reportes: PDF (QuestPDF), Excel (ClosedXML), impresión de tickets térmicos (ESCPOS_NET).
