# AgendeAqui - AI Assistant Instructions

## Project

Multi-tenant Scheduling-as-a-Service API targeting the Brazilian market.

## Stack

- .NET 10 LTS, C# 13, PostgreSQL 16, EF Core + Dapper (CQRS)
- RabbitMQ 3.13, Mediator (source generators), Mapperly (compile-time)
- FluentValidation, xUnit v3 + FluentAssertions + NSubstitute

## Architecture Rules

- Clean Architecture: Domain -> Application -> Infrastructure -> Api
- **Domain** has ZERO dependencies on other layers (only Mediator.Abstractions)
- **Application** depends only on Domain
- **Infrastructure** depends on Application (and transitively Domain)
- **Api** depends on Application and Infrastructure
- Never reference Infrastructure from Domain or Application

## Conventions

### Domain Layer
- Entities use `private` constructors + `static Create()` factory methods
- Value Objects extend `ValueObject`, return `Result<T>` from Create()
- Aggregate Roots extend `AggregateRoot`, raise domain events via `RaiseDomainEvent()`
- All tenant-scoped entities extend `TenantEntity`

### Application Layer
- Commands implement `ICommand` or `ICommand<T>`, handled by `ICommandHandler`
- Queries implement `IQuery<T>`, handled by `IQueryHandler`
- Commands use EF Core (via repositories), Queries use Dapper (via ISqlConnectionFactory)
- Validators use FluentValidation, auto-registered

### Infrastructure Layer
- EF Core configurations in `Persistence/Configurations/`
- Table names: snake_case (e.g., `appointments`, `professionals`)
- Column names: snake_case (e.g., `tenant_id`, `created_at`)
- RLS via TenantInterceptor sets `app.current_tenant_id` session variable

### Api Layer
- Minimal API endpoints in `Endpoints/` folder
- Tenant resolved via `X-Tenant-Id` header
- Global exception handling returns RFC 7807 ProblemDetails

## Commands

```bash
# Run infrastructure
docker-compose up -d

# Build
dotnet build

# Test
dotnet test

# Run API
dotnet run --project src/AgendeAqui.Api
```

## Migrations

```bash
# Create new migration
dotnet ef migrations add MigrationName -p src/AgendeAqui.Infrastructure -s src/AgendeAqui.Api --output-dir Persistence/Migrations

# Apply migrations (production)
dotnet ef database update -p src/AgendeAqui.Infrastructure -s src/AgendeAqui.Api

# Rollback last migration
dotnet ef migrations remove -p src/AgendeAqui.Infrastructure -s src/AgendeAqui.Api
```

**Note:** Integration tests use `EnsureCreatedAsync()` (no migrations needed for tests).

## File Organization

- Group by feature/aggregate (e.g., `Appointments/`, `Tenants/`)
- One class per file
- Namespace matches folder structure

## Frontend (React Dashboard)

- Located in `frontend/` directory (monorepo)
- Stack: React 19, TypeScript, Vite, TailwindCSS 4, TanStack Query, React Router, Recharts, @microsoft/signalr, date-fns
- The frontend is a pure SPA client of the API — no special/exclusive endpoints
- API types generated from OpenAPI spec via `openapi-typescript`

```bash
# Install dependencies
cd frontend && npm install

# Dev server (proxies API to localhost:5001)
cd frontend && npm run dev

# Build production
cd frontend && npm run build

# Generate API types from OpenAPI
cd frontend && npm run generate-api

# Run tests
cd frontend && npm test
```

## Design Docs

- Design documents saved in `docs/plans/`
- Implementation plans saved in `docs/plans/`
- Current: `docs/plans/2026-03-09-dashboard-react-sync-design.md`
- Plan: `docs/plans/2026-03-09-dashboard-react-sync-implementation.md`

## Two Products

1. **AgendeAqui API** — produto core, vendido standalone para chatbots/ERPs/apps
2. **AgendeAqui Dashboard** — frontend React opcional, consome a mesma API

Todos os clientes (dashboard, chatbot, parceiro) usam a mesma API com os mesmos endpoints.
