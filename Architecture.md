
---

## 1. Building Blocks

Reusable libraries that provide cross-cutting infrastructure code without leaking **domain logic**.

### `BuildingBlocks.Persistence`
- **Role**: Provide common persistence patterns for EF Core + PostgreSQL.
- **Contents**:
  - `DbOptions.cs`: encapsulates connection string + schema settings.
  - `OutboxMessage.cs`: defines outbox entity for reliable messaging.
  - Outbox dispatcher helpers for background publishing.
- **Boundary**: Never reference domain entities. Only provides abstractions/utilities to infrastructure layers of services.

### `BuildingBlocks.Messaging`
- **Role**: Provide abstractions and helpers for inter-service messaging.
- **Contents**:
  - Interfaces like `IIntegrationEvent`.
  - MassTransit setup helpers (RabbitMQ configuration, consumer registration).
- **Boundary**: Only contracts and setup code. No service-specific message handling.

### `BuildingBlocks.Web`
- **Role**: Cross-cutting web utilities.
- **Contents**:
  - Logging setup (Serilog).
  - Health check extensions.
  - OpenTelemetry setup helpers.
- **Boundary**: Shared **framework** code only. No domain/business logic.

---

## 2. Services

Each service is a **bounded context**. Each has four projects: `Api`, `Application`, `Domain`, `Infrastructure`.

### Common Boundaries Across All Services
- Each service has its **own database schema** in PostgreSQL.
- Each service owns its **entities, rules, and persistence**.
- Services communicate via **REST (synchronous)** or **events (asynchronous)**.
- **No direct cross-schema queries**: data exchange happens via events and projections.

### `*.Domain`
- **Role**: Core business logic and domain model.
- **Contents**:
  - Entities (e.g., `Product`).
  - Value Objects.
  - Domain services and invariants.
- **Boundary**: No dependencies on EF Core, Web, or infrastructure libraries. Pure business code.

### `*.Application`
- **Role**: Application logic orchestrating domain behavior.
- **Contents**:
  - Command/Query handlers (CQRS pattern).
  - DTO mappers and validators.
  - Transaction scripts that use domain entities.
- **Boundary**: References `Domain`, but **not** Infrastructure or Web. Stays thin.

### `*.Infrastructure`
- **Role**: Technical implementation details of persistence and integration.
- **Contents**:
  - `DbContext` (e.g., `CatalogDbContext`).
  - Entity type configurations (EF Core mappings).
  - Repository implementations (if used).
  - Event outbox table mappings.
  - Design-time `DbContextFactory` (optional).
- **Boundary**: Depends on EF Core + Npgsql. References `Domain`. Exposes DI extension methods for `Api` to use.

### `*.Api`
- **Role**: Service entrypoint (the executable project).
- **Contents**:
  - Minimal API endpoints / Controllers.
  - Program bootstrap (logging, DI, health checks, OpenAPI).
  - Swagger UI.
  - Event consumers (via MassTransit).
- **Boundary**: Orchestrates dependencies. Calls into Application/Infrastructure. No direct business logic.

---

## 3. Deployment Folder

### `deploy/compose/infra.yml`
- **Role**: Provides local development infrastructure services.
- **Contents**:
  - Postgres (with one `erp` DB, service schemas created per service).
  - RabbitMQ (for messaging).
  - Jaeger (for distributed tracing).
  - PgAdmin (for DB admin).

### `deploy/compose/dev.yml`
- **Role**: Service-level containers for local dev.
- **Contents**:
  - Builds and runs individual microservices against the infra stack.

---

## 4. Tests Folder

### `tests/`
- **Unit Tests**: Validate domain logic (no database).
- **Integration Tests**: Boot minimal service + test persistence and APIs.
- **Contract/Component Tests**: Validate service interfaces (OpenAPI, AsyncAPI).

---

## 5. Boundaries Recap

- **BuildingBlocks**: Cross-cutting technical helpers only. No domain leakage.
- **Domain**: Pure business model. No EF Core, no ASP.NET, no messaging.
- **Application**: Use-cases and orchestration. Calls Domain, knows nothing about EF or RabbitMQ.
- **Infrastructure**: Implements persistence, messaging outbox, and repository patterns. Depends on EF, Npgsql, MassTransit. No UI/API concerns.
- **Api**: The HTTP/messaging entrypoint. Configures the service, exposes endpoints, wires DI.

---

## 6. Example: Catalog Service Boundaries

- **Catalog.Domain**
  - `Product` entity, rules like "SKU must be unique".
- **Catalog.Application**
  - Handler for `CreateProductCommand` which validates SKU uniqueness.
- **Catalog.Infrastructure**
  - `CatalogDbContext` (with schema = `catalog`).
  - EF mapping for `Product`.
- **Catalog.Api**
  - `/v1/products` endpoints.
  - Applies migrations at startup.
  - Uses DI to inject Application and Infrastructure.

---

## 7. General Principles

- Each service owns **its schema + migrations**.
- Each service should be **deployable independently** (API project = container entrypoint).
- **Communication**: Async messaging preferred for workflows; REST for queries/commands between services.
- **Observability**: Every API has `/healthz`, logging, tracing.
- **Scalability**: Services scale independently since boundaries are respected.

---

## 8. Next Steps

- Repeat the `Domain/Application/Infrastructure/Api` pattern for `Inventory`, `Sales`, `Procurement`, `Fulfillment`.
- Add messaging (RabbitMQ + Outbox) for cross-service workflows.
- Expand `BuildingBlocks` as more cross-cutting needs arise (e.g., caching, validation).

---

**End of Document**
"""

# Save to markdown file
output_path = "/mnt/data/ERP_Project_Structure.md"
with open(output_path, "w", encoding="utf-8") as f:
    f.write(doc_content)

output_path
