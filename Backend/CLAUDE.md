# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

LaundryManagement is a **pure Domain-Driven Design (DDD)** implementation for a laundry/dry cleaning management system, built with .NET 8 using Clean Architecture principles. The system implements CQRS with MediatR, uses dual database access patterns (EF Core for writes, Dapper for reads), and maintains complete separation between domain and infrastructure concerns.

## Build & Run Commands

### Base de datos (PostgreSQL via Docker)
```bash
# Levantar PostgreSQL (desde Backend/)
docker compose up -d

# Verificar que está listo
docker compose ps

# Aplicar seed de datos iniciales
docker exec -i lavanderia_postgres psql -U lavanderia_user -d LavanderiaDB < seed_admin_user.sql
# Usuarios: admin / Admin123!  |  empleado1 / Empleado123!
```

### Build Solution
```bash
dotnet build LaundryManagement.sln
```

### Run API (Development)
```bash
cd src/LaundryManagement.API
dotnet run
```
API runs on `https://localhost:7037` and `http://localhost:5065` with Swagger UI available at the root URL.

### Build Specific Project
```bash
dotnet build src/LaundryManagement.Domain/LaundryManagement.Domain.csproj
dotnet build src/LaundryManagement.Application/LaundryManagement.Application.csproj
dotnet build src/LaundryManagement.Infrastructure/LaundryManagement.Infrastructure.csproj
dotnet build src/LaundryManagement.API/LaundryManagement.API.csproj
```

### Restore NuGet Packages
```bash
dotnet restore
```

### Database Migrations
```bash
# Add new migration
cd src/LaundryManagement.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../LaundryManagement.API

# Update database
dotnet ef database update --startup-project ../LaundryManagement.API

# Generate migration SQL script
dotnet ef migrations script --startup-project ../LaundryManagement.API
```

## Architecture Overview

### Layer Dependencies (Clean Architecture)
```
API Layer (Presentation)
    ↓ depends on
Application Layer (Use Cases: Commands/Queries)
    ↓ depends on
Domain Layer (Business Logic - PURE, no external dependencies)
    ↑ knows about (via InternalsVisibleTo)
Infrastructure Layer (EF Core, Dapper, Persistence)
```

### Critical Architectural Principle: Pure Domain

**The Domain layer has ZERO external dependencies** and contains no infrastructure concerns. This is achieved through:

1. **Separate Entity Models**:
   - Domain entities (e.g., `OrderPure`) contain business logic
   - Infrastructure entities (e.g., `Ordene`) handle EF Core persistence
   - `OrderMapper` translates between them (anti-corruption layer)

2. **InternalsVisibleTo Pattern**:
   ```xml
   <!-- Domain.csproj -->
   <InternalsVisibleTo Include="LaundryManagement.Infrastructure" />
   ```
   This allows Infrastructure to call internal factory methods like `OrderPure.Reconstitute()` without exposing them publicly.

3. **Repository Interfaces in Domain**: `IOrderRepository` is defined in Domain, implemented in Infrastructure

### CQRS Implementation

**Commands (Write Operations)**:
- Located in `Application/Commands/`
- Each command has: Command class, CommandHandler, CommandValidator (FluentValidation)
- Example: `CreateOrderCommand` → `CreateOrderCommandHandler` → invokes domain aggregates
- Handlers coordinate domain logic and persistence

**Queries (Read Operations)**:
- Located in `Application/Queries/`
- Return DTOs, not domain aggregates
- Can use Dapper for performance-critical reads
- Example: `GetOrderByIdQuery` → `GetOrderByIdQueryHandler`

### Dual Database Access Pattern

1. **EF Core (Writes)**: Transactional operations with retry logic for transient failures
   - Used in Repositories for aggregate persistence
   - Configured with `EnableRetryOnFailure` (maxRetryCount: 5)

2. **Dapper (Reads)**: Performance-optimized reads via stored procedures
   - Used in Service implementations (IOrdenService, IPagoService, etc.)
   - `IDbConnectionFactory` provides connections

### Domain Patterns

**Aggregates**:
- `OrderPure`: Root aggregate managing order lifecycle, line items, and discounts
- Factory method: `OrderPure.Create()`
- Reconstitution: `OrderPure.Reconstitute()` (internal, for repository use)
- Enforces all business invariants through methods, not property setters

**Value Objects**:
- `Money`: Currency-aware calculations with operators (+, -, *, percentage)
- `OrderId`, `ClientId`: Strongly-typed IDs with validation
- `OrderFolio`: Auto-generated format `ORD-YYYYMMDD-NNNN` with regex validation
- All immutable, compared by value not reference

**Domain Events**:
- Raised by aggregates during state changes: `OrderCreated`, `OrderLineItemAdded`, `OrderStatusChanged`, etc.
- Stored in `_domainEvents` collection on aggregate root
- Dispatched automatically during `SaveChangesAsync()`
- Enable side effects without coupling

**Entities**:
- Base class: `Entity<TId>` (compared by ID)
- Rich behavior, encapsulated state
- Example: `OrderLineItem`, `OrderDiscount`

### Base Classes Reference

- `AggregateRoot<TId>`: Domain events + entity equality
- `Entity<TId>`: Identity-based equality
- `ValueObject`: Structural equality (all fields)
- `DomainEvent`: Base for all domain events
- `BaseException`: Custom exception hierarchy with HTTP status codes

### Exception Hierarchy

All domain exceptions inherit from `BaseException`:
- `ValidationException` (400): Input validation failures
- `NotFoundException` (404): Entity not found
- `BusinessRuleException` (409): Domain rule violations
- `DatabaseException` (500): Persistence errors

The `GlobalExceptionHandlerMiddleware` maps exceptions to HTTP responses automatically.

## Key Conventions

### 1. Respect Layer Boundaries

**Never** let Domain depend on Infrastructure:
```csharp
// ❌ WRONG - Domain referencing infrastructure entity
public class OrderPure {
    private readonly Ordene _entity; // NO!
}

// ✅ CORRECT - Pure domain entity
public class OrderPure {
    private readonly List<OrderLineItem> _lineItems; // Domain entities only
}
```

### 2. Always Use Aggregates for Business Logic

```csharp
// ❌ WRONG - Direct entity manipulation
var ordene = new Ordene { Subtotal = -100 }; // No validation!
context.Ordenes.Add(ordene);

// ✅ CORRECT - Use aggregate
var order = OrderPure.Create(clientId, promisedDate, receivedBy, statusId);
order.AddLineItem(serviceId, quantity, unitPrice); // Validates + calculates
await repository.AddAsync(order);
```

### 3. Mapper Usage (Infrastructure Only)

The `OrderMapper` class should **only** be called in:
- Repository implementations (`OrderRepositoryPure`)
- Infrastructure services

Never expose infrastructure entities outside Infrastructure layer.

### 4. Value Objects for Domain Concepts

```csharp
// ❌ WRONG - Primitive obsession
public void AddLineItem(decimal unitPrice) { }

// ✅ CORRECT - Rich domain type
public void AddLineItem(Money unitPrice) { }
```

### 5. Command/Query Naming

- Commands: `{Verb}{Entity}Command` (e.g., `CreateOrderCommand`)
- Queries: `Get{Entity}By{Criteria}Query` (e.g., `GetOrderByIdQuery`)
- Handlers: `{CommandName}Handler`
- Validators: `{CommandName}Validator`

### 6. Controller Responsibilities

Controllers should:
- Validate HTTP concerns (routing, headers)
- Dispatch to MediatR (`_mediator.Send(command)`)
- Map responses to DTOs
- Return appropriate HTTP status codes

Controllers should **NOT**:
- Contain business logic
- Directly call repositories
- Manipulate domain entities

### 7. Internal Factory Methods

For reconstructing aggregates from database (used by repositories):
```csharp
// In Domain - internal visibility
internal static OrderPure Reconstitute(
    OrderId id,
    OrderFolio folio,
    // ... all properties
) { }

// In Infrastructure Repository
var order = OrderPure.Reconstitute(
    OrderId.From(entity.OrdenId),
    OrderFolio.From(entity.Folio),
    // ...
);
```

## Database Configuration

**Connection String** (appsettings.Development.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=LavanderiaDB;Username=lavanderia_user;Password=lavanderia_pass_dev"
  }
}
```

**Base de datos**: PostgreSQL 16 via Docker (`docker compose up -d` desde `Backend/`)

**EF Core Context**: `LaundryDbContext` en Infrastructure layer (proveedor Npgsql)

**Queries de lectura**: Dapper con SQL directo — todos los identificadores PascalCase van entre comillas dobles (`"Ordenes"`, `"OrdenID"`, etc.)

**Stored Procedures**: Eliminados — reemplazados por EF Core (writes) y Dapper SQL (reads) en los servicios de Infrastructure

## Project Structure

```
LaundryManagement.sln
├── src/
│   ├── LaundryManagement.Domain/          # Pure business logic (no external deps)
│   │   ├── Aggregates/Orders/
│   │   │   ├── OrderPure.cs              # Root aggregate
│   │   │   ├── OrderLineItem.cs          # Child entity
│   │   │   └── OrderDiscount.cs          # Child entity
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── OrderId.cs
│   │   │   ├── OrderFolio.cs
│   │   │   └── ClientId.cs
│   │   ├── DomainEvents/Orders/
│   │   ├── Repositories/
│   │   │   └── IOrderRepository.cs       # Interface only
│   │   ├── Exceptions/
│   │   └── Common/                        # Base classes
│   │
│   ├── LaundryManagement.Application/     # Use cases (CQRS)
│   │   ├── Commands/Orders/
│   │   │   ├── CreateOrderCommand.cs
│   │   │   ├── CreateOrderCommandHandler.cs
│   │   │   └── CreateOrderCommandValidator.cs
│   │   ├── Queries/Orders/
│   │   ├── DTOs/
│   │   ├── Interfaces/                    # Service contracts
│   │   └── DependencyInjection.cs
│   │
│   ├── LaundryManagement.Infrastructure/  # Persistence & mapping
│   │   ├── Persistence/
│   │   │   ├── LaundryDbContext.cs       # EF Core context
│   │   │   ├── Entities/                 # EF entities (NOT domain)
│   │   │   │   ├── Ordene.cs             # Infrastructure entity
│   │   │   │   ├── OrdenesDetalle.cs
│   │   │   │   └── ...
│   │   │   └── Configurations/           # EF Fluent API configs
│   │   ├── Repositories/
│   │   │   └── OrderRepositoryPure.cs    # IOrderRepository impl
│   │   ├── Mappers/
│   │   │   └── OrderMapper.cs            # Domain ↔ Infrastructure translation
│   │   ├── Services/                      # Stored procedure implementations
│   │   ├── QueryHandlers/
│   │   └── DependencyInjection.cs
│   │
│   └── LaundryManagement.API/             # REST API
│       ├── Controllers/
│       │   ├── OrdenesController.cs
│       │   ├── PagosController.cs
│       │   ├── CortesCajaController.cs
│       │   └── ReportesController.cs
│       ├── Middleware/
│       │   └── GlobalExceptionHandlerMiddleware.cs
│       ├── Program.cs                     # DI setup & middleware pipeline
│       └── Properties/launchSettings.json
│
├── DDD-PURE-IMPLEMENTATION.md             # Architecture decision record
└── LaundryManagement.sln
```

## Adding New Features

### 1. Create Domain Aggregate (if new bounded context)

```csharp
// In Domain/Aggregates/{Context}/{Aggregate}.cs
public sealed class MyAggregate : AggregateRoot<MyAggregateId>
{
    // Private fields for encapsulation
    private readonly List<MyEntity> _items = new();

    // Factory method
    public static MyAggregate Create(/* params */) { }

    // Business methods that enforce invariants
    public void DoSomething() { }

    // Internal reconstitution for repository
    internal static MyAggregate Reconstitute(/* params */) { }
}
```

### 2. Create Value Objects

```csharp
// In Domain/ValueObjects/{Name}.cs
public sealed class MyValue : ValueObject
{
    public string Value { get; }

    private MyValue(string value) { Value = value; }

    public static MyValue From(string value)
    {
        // Validation
        return new MyValue(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### 3. Create Repository Interface (Domain)

```csharp
// In Domain/Repositories/I{Aggregate}Repository.cs
public interface IMyAggregateRepository
{
    Task<MyAggregate?> GetByIdAsync(MyAggregateId id);
    Task AddAsync(MyAggregate aggregate);
    Task UpdateAsync(MyAggregate aggregate);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}
```

### 4. Implement Repository (Infrastructure)

```csharp
// In Infrastructure/Repositories/{Aggregate}RepositoryPure.cs
public class MyAggregateRepositoryPure : IMyAggregateRepository
{
    private readonly LaundryDbContext _context;

    public async Task<MyAggregate?> GetByIdAsync(MyAggregateId id)
    {
        var entity = await _context.MyEntities.FindAsync(id.Value);
        return entity == null ? null : MyAggregateMapper.ToDomain(entity);
    }

    public async Task AddAsync(MyAggregate aggregate)
    {
        var entity = MyAggregateMapper.ToInfrastructure(aggregate);
        await _context.MyEntities.AddAsync(entity);
    }
}
```

### 5. Create Mapper (Infrastructure)

```csharp
// In Infrastructure/Mappers/{Aggregate}Mapper.cs
public static class MyAggregateMapper
{
    public static MyAggregate ToDomain(MyEntity entity) { }
    public static MyEntity ToInfrastructure(MyAggregate aggregate) { }
}
```

### 6. Create Command/Query (Application)

```csharp
// In Application/Commands/{Context}/{Action}Command.cs
public record CreateMyAggregateCommand(/* params */) : IRequest<MyAggregateId>;

// Handler
public class CreateMyAggregateCommandHandler : IRequestHandler<CreateMyAggregateCommand, MyAggregateId>
{
    private readonly IMyAggregateRepository _repository;

    public async Task<MyAggregateId> Handle(CreateMyAggregateCommand cmd, CancellationToken ct)
    {
        var aggregate = MyAggregate.Create(/* params */);
        await _repository.AddAsync(aggregate);
        await _repository.SaveChangesAsync(ct);
        return aggregate.Id;
    }
}

// Validator
public class CreateMyAggregateCommandValidator : AbstractValidator<CreateMyAggregateCommand>
{
    public CreateMyAggregateCommandValidator()
    {
        RuleFor(x => x.Field).NotEmpty();
    }
}
```

### 7. Create Controller Endpoint (API)

```csharp
// In API/Controllers/{Context}Controller.cs
[ApiController]
[Route("api/[controller]")]
public class MyAggregatesController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    [ProducesResponseType(typeof(MyAggregateId), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMyAggregateCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = id.Value }, id);
    }
}
```

### 8. Register in DI (Infrastructure)

```csharp
// In Infrastructure/DependencyInjection.cs
services.AddScoped<IMyAggregateRepository, MyAggregateRepositoryPure>();
```

## Common Pitfalls to Avoid

1. **Don't bypass aggregates**: Always modify state through aggregate methods, never directly through child entities
2. **Don't expose internal collections**: Use `IReadOnlyCollection<T>` or return copies
3. **Don't create anemic domain models**: Put business logic in domain, not in handlers
4. **Don't use `public set`**: Use private setters or readonly fields in domain entities
5. **Don't forget validation**: Validate in value object constructors and aggregate methods
6. **Don't leak infrastructure types**: Never return `Ordene` (EF entity) from Application layer
7. **Don't ignore domain events**: Use them for side effects and auditing

## Testing Strategy

While no test projects currently exist, the architecture supports:

**Unit Tests (Domain)**:
```csharp
// No infrastructure needed - pure logic testing
var order = OrderPure.Create(clientId, promisedDate, receivedBy, statusId);
order.AddLineItem(serviceId, quantity, Money.FromDecimal(100));
Assert.Equal(100m, order.Total.Amount);
```

**Integration Tests (Application)**:
```csharp
// Mock repositories
var mockRepo = new Mock<IOrderRepository>();
var handler = new CreateOrderCommandHandler(mockRepo.Object);
var result = await handler.Handle(command, default);
```

**E2E Tests (API)**:
```csharp
// WebApplicationFactory<Program>
var client = _factory.CreateClient();
var response = await client.PostAsJsonAsync("/api/ordenes/v2", command);
```

## Dependency Versions

- .NET SDK: 8.0+
- Entity Framework Core: 8.0+
- SQL Server (LocalDB or full instance)
- MediatR: Latest stable
- FluentValidation: Latest stable
- AutoMapper: Latest stable
- Dapper: Latest stable

## Additional Documentation

- `DDD-PURE-IMPLEMENTATION.md`: Detailed explanation of pure DDD implementation with OrderPure aggregate
- `src/LaundryManagement.Infrastructure/Persistence/Entities/README.md`: Explanation of infrastructure entities vs domain entities
