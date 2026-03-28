# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Lavanderia 2.0 is a full-stack laundry/dry cleaning management system with:
- **Backend**: `Backend/` — ASP.NET Core 8 REST API with DDD + Clean Architecture
- **Frontend**: `Frontend/` — React 19 + TypeScript SPA

Each sub-project has its own detailed `CLAUDE.md`. Read the relevant one before working in that area.

## Quick Start

### Backend (from `Backend/`)
```bash
dotnet restore
dotnet build LaundryManagement.sln
cd src/LaundryManagement.API && dotnet run
# API: https://localhost:7037 | Swagger at root URL
```

### Frontend (from `Frontend/`)
```bash
npm install
npm run dev       # http://localhost:5173
npm run build     # Type-check + production build
npm run lint      # ESLint
```

### Database Migrations (from `Backend/src/LaundryManagement.Infrastructure`)
```bash
dotnet ef migrations add <Name> --startup-project ../LaundryManagement.API
dotnet ef database update --startup-project ../LaundryManagement.API
```

## Architecture Summary

### Backend — Clean Architecture + DDD

**Layer dependencies**: Domain ← Application ← Infrastructure + API

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `LaundryManagement.Domain` | Pure business logic — Aggregates, Value Objects, Domain Events. Zero external dependencies. |
| Application | `LaundryManagement.Application` | CQRS (Commands/Queries via MediatR), FluentValidation, AutoMapper DTOs |
| Infrastructure | `LaundryManagement.Infrastructure` | EF Core (writes), Dapper (reads via stored procedures), repository implementations |
| API | `LaundryManagement.API` | REST controllers, JWT auth middleware, global exception handler, Serilog |

**Key patterns**:
- CQRS: Commands mutate state via EF Core + domain aggregates; Queries use Dapper for performance
- Domain entities are mapped to/from EF entities in the Infrastructure layer (they are separate classes)
- All domain exceptions map to specific HTTP status codes via the global exception handler

### Frontend — Feature-Based Vertical Slices

Features are self-contained in `src/features/<name>/` — each owns its own components, hooks, stores, and API calls.

**Feature modules**: `auth`, `orders`, `customers`, `services`, `users`, `dashboard`

**Shared layer** (`src/shared/`): code used by 2+ features — UI components, hooks, Zustand stores, utils, permissions config

**API layer** (`src/api/`): centralized Axios instance with interceptors; one module per resource

**State**: Zustand + Immer for global state per feature; local `useState` for UI-only state

## Database

SQL Server — database name `LavanderiaDB`. Connection via Windows auth (Trusted_Connection).
EF Core migrations in `Backend/src/LaundryManagement.Infrastructure/Migrations/`.
Stored procedures called via Dapper for orders, payments, cash closings, and reports.

## Key Business Domains

- **Órdenes** (Orders): core aggregate — order lifecycle, line items, discounts, folios
- **Clientes** (Customers): CRM
- **Servicios** (Services): catalog of laundry services with garment types and pricing
- **Pagos** (Payments): payment records linked to orders
- **Cortes de Caja** (Cash Closings): end-of-day reconciliation
- **Usuarios / Roles / Permisos**: JWT auth with role-based access control
- **Reportes**: PDF (QuestPDF), Excel (ClosedXML), and thermal receipt printing (ESCPOS_NET)
