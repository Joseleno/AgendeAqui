# AgendeAqui

Scheduling-as-a-Service multi-tenant API for the Brazilian market.

## Stack

- .NET 10 (LTS) + ASP.NET Core Minimal APIs
- PostgreSQL 16 with Row-Level Security (RLS)
- RabbitMQ 3.13 for async messaging
- EF Core (writes) + Dapper (reads) - CQRS pattern
- Mediator (source generators) for in-process messaging
- Mapperly for compile-time object mapping
- FluentValidation for request validation
- xUnit + FluentAssertions + NSubstitute for testing

## Architecture

Clean Architecture with 4 layers:

```
Api -> Application -> Domain
         ^
Infrastructure
```

- **Domain**: Entities, Value Objects, Aggregates, Domain Events, Repository interfaces
- **Application**: CQRS commands/queries, handlers, pipeline behaviors, validation
- **Infrastructure**: EF Core, Dapper, RabbitMQ, multi-tenancy (RLS), Mapperly mappers
- **Api**: Minimal API endpoints, middleware, health checks

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-docker/)

### Run Infrastructure

```bash
docker-compose up -d
```

This starts:
- PostgreSQL 16 on port 5432
- RabbitMQ on ports 5672 (AMQP) and 15672 (Management UI)

### Build & Run

```bash
dotnet build
dotnet run --project src/AgendeAqui.Api
```

### Run Tests

```bash
dotnet test
```

### Health Check

```
GET /health
```

## API Endpoints

### Tenants

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/tenants` | Create a new tenant |
| GET | `/api/v1/tenants/{id}` | Get tenant by ID |

### Create Tenant

```bash
curl -X POST http://localhost:5000/api/v1/tenants \
  -H "Content-Type: application/json" \
  -d '{"name":"My Clinic","slug":"my-clinic","plan":"Free"}'
```

## Multi-Tenancy

- Row-Level Security (RLS) in PostgreSQL ensures data isolation
- Tenant resolved via `X-Tenant-Id` header
- EF Core Global Query Filters + Dapper session variable for RLS

## Project Structure

```
src/
  AgendeAqui.Domain/          # Domain layer (entities, VOs, events)
  AgendeAqui.Application/     # Application layer (CQRS, behaviors)
  AgendeAqui.Infrastructure/  # Infrastructure (EF Core, RabbitMQ)
  AgendeAqui.Api/             # API layer (endpoints, middleware)
tests/
  AgendeAqui.Domain.UnitTests/
  AgendeAqui.Application.UnitTests/
  AgendeAqui.ArchTests/
```
