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

## File Organization

- Group by feature/aggregate (e.g., `Appointments/`, `Tenants/`)
- One class per file
- Namespace matches folder structure
