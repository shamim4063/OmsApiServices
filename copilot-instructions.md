# Copilot Instructions — ERP Microservices Backend (.NET 9 + PostgreSQL)

These instructions guide AI coding assistants (Copilot, ChatGPT, etc.) on how to propose code, complete tasks, and navigate this repository. **Follow these boundaries and conventions.**

---

## Repository Architecture (Authoritative)

```
OmsApiServices/
├─ Src/
│  ├─ BuildingBlocks/
│  │  ├─ Persistence/
│  │  ├─ Messaging/
│  │  └─ Web/
│  ├─ Services/
│  │  ├─ Catalog/
│  │  │  ├─ Catalog.Api/
│  │  │  ├─ Catalog.Application/
│  │  │  ├─ Catalog.Domain/
│  │  │  └─ Catalog.Infrastructure/
│  │  └─ ... (inventory, sales, procurement, fulfillment, etc.)
├─ deploy/
│  └─ compose/
│     ├─ infra.yml
│     └─ dev.yml
├─ Tests/
└─ OmsApiServices.sln
```

### Boundaries per Layer (MUST NOT be violated)

- **Domain (`*.Domain`)**
  - Pure business model: entities, value objects, domain services, invariants.
  - **No** EF Core, ASP.NET, MassTransit, or framework dependencies.
  - Keep constructors/behaviors expressive and validated.
- **Application (`*.Application`)**
  - Use cases (CQRS handlers), validation, mapping, orchestration of domain.
  - **No** persistence or transport specifics; call abstractions.
- **Infrastructure (`*.Infrastructure`)**
  - EF Core DbContext + mappings, repositories, data access, outbox persistence.
  - Event publisher implementations (when needed).
  - **No** web endpoints or UI logic.
- **API (`*.Api`)**
  - Minimal API/controllers, DI composition root, logging, health, Swagger.
  - May host message consumers (MassTransit) and background services.
  - **No** domain rules or direct SQL; call Application or Infrastructure through DI.
- **Building Blocks (`src/building-blocks/*`)**
  - Reusable, **non-domain** helpers: persistence options, outbox entity/dispatcher, messaging setup, web utilities.
  - Never depend on service-specific domain types.

---

## Database Policy (Critical)

- One PostgreSQL instance for dev; **one schema per service** (e.g., `catalog`, `inventory`, `sales`).
- Each service has its own **DbContext** and **EF Core migrations** stored in `*.Infrastructure`.
- **No cross-schema queries or foreign keys** between services.
- IDs are GUIDs (prefer v7 if available). Use optimistic concurrency where applicable.
- Use JSONB for flexible attributes; avoid leaking this detail into Domain.

### EF Core Tooling

- **PMC**: set *Default project* to `*.Infrastructure` (where `DbContext` lives).
- Use the API as startup (or a design-time factory):
  ```powershell
  Add-Migration <Name> -StartupProject <Service>.Api
  Update-Database -StartupProject <Service>.Api
  ```
- Provider and tools must match EF Core 9 (e.g., `Npgsql.EntityFrameworkCore.PostgreSQL 9.x`).
- Migrations history table lives in the service schema (`__EFMigrationsHistory`).

---

## Messaging & Integration (When Introduced)

- Prefer async events via RabbitMQ/MassTransit.
- Use **outbox pattern**: `_outbox` table in each service schema; background dispatcher publishes to the bus.
- Publish domain/reliable integration events like `Catalog.ProductCreated.v1`.
- Consumers project data into local read models; do not query other services’ schemas.

---

## API Conventions

- Minimal API endpoints under versioned routes: `/v1/...`.
- Use `Idempotency-Key` header for write endpoints that can be retried (orders/payments).
- Health check at `/healthz` with NpgSql check for the service’s connection string.
- Swagger enabled for every API; OpenAPI docs committed per service under `src/contracts/openapi` (when added).

---

## Coding Conventions

- **TargetFramework**: `net9.0` for all projects.
- Enable nullable reference types; treat warnings as errors in CI.
- Prefer MediatR (or equivalent) for CQRS inside `Application` (optional if simple).
- Validation via FluentValidation (in Application) or custom validators.
- Mapping via Mapster/AutoMapper in Application (keep DTOs out of Domain).
- Use `HasDefaultSchema("<service>")` or `MigrationsHistoryTable(..., "<service>")` in DbContext configuration.
- Logging: Serilog in API projects; structured logs with correlation IDs.

---

## What Copilot Should Generate (Examples)

### Domain
- New entity/value object with invariants and methods that keep state consistent.
- Domain service for complex rules spanning multiple entities.

### Application
- Command/Query handlers with validation and unit tests.
- DTOs mapped from Domain types without leaking EF Core types.

### Infrastructure
- `DbContext` entity configurations, migrations, repositories.
- Outbox persistence configuration and a hosted dispatcher service.

### API
- Minimal API endpoints that delegate to Application, returning ProblemDetails on error.
- Health checks, Swagger configuration, DI registrations using extension methods.

---

## What Copilot Must Not Do

- Do **not** call another service’s database directly or add cross-schema FKs.
- Do **not** place business rules in controllers, DbContext, or EF configurations.
- Do **not** add shared “domain” libraries; only share **contracts/utilities** under building-blocks.
- Do **not** change migration assembly/paths unless instructed.

---

## File/Folder Expectations (Per Service)

- `*.Api/Program.cs`: bootstrap, DI, health checks, swagger, migrations on startup (dev only).
- `*.Infrastructure/<Service>DbContext.cs`: EF Core mappings; `_outbox` mapping.
- `*.Infrastructure/Migrations/*`: EF migrations for the service schema.
- `*.Domain/*`: entities, value objects, domain services only.
- `*.Application/*`: use-cases, validators, mappers (may reference Domain only).

---

## PR Checklist (for AI-Generated Changes)

- [ ] Layer boundaries respected (Domain ↔ Application ↔ Infrastructure ↔ Api).
- [ ] New DB objects exist only in the service’s schema.
- [ ] Migrations are added to `*.Infrastructure/Migrations` and compile.
- [ ] Health checks and Swagger still pass; app builds and runs.
- [ ] No direct dependencies from Domain to EF Core / ASP.NET.
- [ ] Unit tests for Domain/Application logic (if applicable).

---

## Useful Snippets

**Register DbContext with schema-specific migrations history:**
```csharp
services.AddDbContext<CatalogDbContext>(opt =>
    opt.UseNpgsql(db.ConnectionString, npg =>
        npg.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));
```

**Minimal API endpoints pattern:**
```csharp
app.MapGet("/v1/products", async (CatalogDbContext db) =>
    await db.Products.AsNoTracking().ToListAsync());

app.MapPost("/v1/products", async (CatalogDbContext db, ProductDto dto) =>
{
    var p = new Product(dto.Sku, dto.Name, dto.Description, dto.ImageMainUrl);
    db.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/v1/products/{p.Id}", new { p.Id });
});
```

**Outbox entity (shared):**
```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string Type { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DateTime? ProcessedAt { get; set; }
}
```

---

## Local Dev Workflow

1. Start infra: `docker compose -f deploy/compose/infra.yml up -d`
2. Set **PMC Default project** = `*.Infrastructure`
3. `Add-Migration <Name> -StartupProject <Service>.Api`
4. `Update-Database -StartupProject <Service>.Api`
5. F5 on `<Service>.Api` and hit `/swagger` and `/healthz`

---

Adhere to this guide so generated code remains consistent, testable, and easy to scale.
