
# ERP Microservices Backend (Monorepo) – Project Structure & Responsibilities

This document explains the **solution structure**, the **role and responsibilities** of each project/folder, and the **boundaries** that enforce clean microservice design.  
Target stack: **.NET 9**, **PostgreSQL**, **Docker**, **EF Core 9**, **MassTransit (RabbitMQ)**.

---

## High-Level Principles

- **Microservice per bounded context**: each service has its own API, Application, Domain, Infrastructure projects.
- **Database-per-service**: all services share one Postgres instance, but each owns its schema and migrations.
- **Clean boundaries**: no domain code is shared; only contracts/utilities live in `building-blocks`.
- **Independently deployable**: each service builds its own container image and can be started individually.

---

## Solution Layout

```
erp.sln
│
├─ src/
│  ├─ building-blocks/
│  │  ├─ Persistence/
│  │  ├─ Messaging/
│  │  └─ Web/
│  │
│  ├─ services/
│  │  ├─ catalog/
│  │  │  ├─ Catalog.Api/
│  │  │  ├─ Catalog.Application/
│  │  │  ├─ Catalog.Domain/
│  │  │  └─ Catalog.Infrastructure/
│  │  └─ ... (other services: inventory, sales, procurement, etc.)
│  │
│  └─ contracts/ (optional: OpenAPI/AsyncAPI specs, DTOs)
│
├─ deploy/
│  ├─ compose/
│  │  ├─ infra.yml
│  │  └─ dev.yml (later)
│  └─ k8s/helm/ (future deploy manifests)
│
└─ tests/
   ├─ component/
   └─ contract/
```

---

## Building Blocks (Shared Libraries)

These projects contain **infrastructure utilities only** (never domain/business logic). They can be reused across services.

### `BuildingBlocks.Persistence`
- **Purpose**: Shared EF Core helpers, DbOptions, Outbox entity/dispatcher.
- **Responsibilities**:
  - Provide a `DbOptions` class to bind connection string & schema from configuration.
  - Define `OutboxMessage` entity and background dispatcher for reliable event publishing.
  - Generic helpers for running migrations at startup.
- **Boundaries**:
  - Must not reference any service domain models.
  - Only contains technical persistence patterns.

### `BuildingBlocks.Messaging`
- **Purpose**: Abstractions & setup for messaging (MassTransit/RabbitMQ).
- **Responsibilities**:
  - Define marker interface(s) for integration events (`IIntegrationEvent`).
  - Provide MassTransit configuration helpers.
  - Outbox dispatcher integration.
- **Boundaries**:
  - No service‑specific events; only contracts and configuration code.

### `BuildingBlocks.Web`
- **Purpose**: Web API startup helpers.
- **Responsibilities**:
  - Configure logging (Serilog).
  - Configure OpenTelemetry tracing.
  - Health check extensions.
  - Common filters/middleware (problem+json, exception handling).
- **Boundaries**:
  - No domain or persistence code.

---

## Service Projects (per bounded context)

Every service is split into **4 projects**, following Clean Architecture.

### 1. `Catalog.Api`
- **Type**: ASP.NET Core Web API (Minimal API or Controllers).
- **Responsibilities**:
  - Hosts the HTTP API endpoints for Catalog service.
  - Startup: wires up DI, EF Core DbContext, messaging, health checks, logging.
  - Defines request/response DTOs (specific to API).
- **Boundaries**:
  - Should not contain domain or EF entities directly in controllers; delegate to Application layer.

### 2. `Catalog.Application`
- **Type**: Class Library.
- **Responsibilities**:
  - Application services, use cases, CQRS handlers (MediatR commands/queries).
  - Input validation (FluentValidation).
  - Transaction coordination (across repositories within same service).
- **Boundaries**:
  - Can reference `Catalog.Domain`.
  - Should not directly depend on EF Core or external frameworks.

### 3. `Catalog.Domain`
- **Type**: Class Library.
- **Responsibilities**:
  - Enterprise/business logic.
  - Entities, Value Objects, Aggregates, Domain Events.
  - Invariants and rules for products, categories, etc.
- **Boundaries**:
  - Pure C# objects, no EF Core or infrastructure code.
  - Must not reference Application/Infrastructure.

### 4. `Catalog.Infrastructure`
- **Type**: Class Library.
- **Responsibilities**:
  - Persistence implementation: `CatalogDbContext`, EF Core entity configs.
  - Migrations live here.
  - Repositories and data access implementations.
  - Integration with messaging (outbox).
- **Boundaries**:
  - References Domain (to map entities).
  - References BuildingBlocks (for persistence/messaging helpers).
  - Should not contain application use cases.

---

## Contracts (optional folder)

- Keep **OpenAPI specifications** (`catalog.openapi.json`) and **AsyncAPI/event schemas**.  
- Useful for cross‑service communication and contract testing.

---

## Deploy Folder

### `deploy/compose/infra.yml`
- Starts shared infrastructure (Postgres, RabbitMQ, pgAdmin, Jaeger).

### `deploy/compose/dev.yml`
- Adds your services on top of infra for local development.

### `deploy/k8s/helm/`
- Placeholder for future Helm charts / Kubernetes manifests.

---

## Tests Folder

- **Component tests**: Spin up service + DB, test API endpoints.
- **Contract tests**: Validate service APIs and events against published contracts.

---

## Boundaries & Rules Recap

- **Database-per-service**: each service owns its schema & migrations.
- **No shared domain models**: only contracts/utilities can be reused.
- **Cross-service communication**: via REST APIs or integration events, not DB joins.
- **Each service independently deployable**: one container per `*.Api`.

---

## Example: Catalog Service Responsibilities

- **Catalog.Api**
  - `/v1/products` endpoints.
  - Health check `/healthz`.
- **Catalog.Application**
  - `CreateProductCommand` handler.
  - Business flow orchestration.
- **Catalog.Domain**
  - `Product` entity with SKU uniqueness rule.
- **Catalog.Infrastructure**
  - `CatalogDbContext` maps `Product` to `catalog.product` table.
  - EF Core migrations under `Migrations/`.

---

## Developer Workflow

1. Start infra:
   ```powershell
   docker compose -f deploy/compose/infra.yml up -d
   ```
2. Run service locally:
   ```powershell
   dotnet watch --project src/services/catalog/Catalog.Api
   ```
3. Add migration:
   ```powershell
   Add-Migration AddProducts -StartupProject Catalog.Api
   Update-Database -StartupProject Catalog.Api
   ```
4. Access Swagger: https://localhost:5001/swagger

---

This structure ensures **clear boundaries, independent evolution, and clean separation** between concerns while staying developer‑friendly in a single monorepo.
