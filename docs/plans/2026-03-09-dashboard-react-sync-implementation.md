# AgendeAqui Dashboard + Sync Bidirecional — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add missing API endpoints (auth, api-keys, filters, idempotency, sync), then build a complete React dashboard (agenda, gestao, relatorios, config) that consumes the existing API.

**Architecture:** Three phases — (1) Backend API gaps, (2) Frontend React scaffold + core pages, (3) Sync bidirecional. Each phase is independently deployable and testable. The frontend is a pure SPA client of the API with zero special endpoints.

**Tech Stack:**
- Backend: .NET 10, C# 13, PostgreSQL 16, EF Core, Dapper, RabbitMQ, FluentValidation
- Frontend: React 19, TypeScript, Vite, TailwindCSS 4, TanStack Query, React Router, Recharts, @microsoft/signalr, date-fns, openapi-typescript

---

## PHASE 1: Backend API Gaps (8 Tasks)

These tasks add the missing endpoints and filters the dashboard needs.

---

### Task 1: Auth Login Endpoint (JWT Token Issuance)

**Why:** The dashboard needs credential-based login. Currently JWT is assumed external — we need `POST /api/v1/auth/login`.

**Files:**
- Create: `src/AgendeAqui.Domain/Users/User.cs`
- Create: `src/AgendeAqui.Domain/Users/UserErrors.cs`
- Create: `src/AgendeAqui.Domain/Abstractions/IUserRepository.cs`
- Create: `src/AgendeAqui.Application/Auth/Login/LoginCommand.cs`
- Create: `src/AgendeAqui.Application/Auth/Login/LoginCommandHandler.cs`
- Create: `src/AgendeAqui.Application/Auth/Login/LoginCommandValidator.cs`
- Create: `src/AgendeAqui.Application/Auth/Login/LoginResponse.cs`
- Create: `src/AgendeAqui.Application/Auth/IJwtTokenGenerator.cs`
- Create: `src/AgendeAqui.Infrastructure/Auth/JwtTokenGenerator.cs`
- Create: `src/AgendeAqui.Infrastructure/Persistence/Repositories/UserRepository.cs`
- Create: `src/AgendeAqui.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- Modify: `src/AgendeAqui.Infrastructure/DependencyInjection.cs` — register UserRepository, JwtTokenGenerator
- Create: `src/AgendeAqui.Api/Endpoints/AuthEndpoints.cs`
- Modify: `src/AgendeAqui.Api/Extensions/WebApplicationExtensions.cs` — add `app.MapAuthEndpoints()`
- Create: `tests/AgendeAqui.Application.UnitTests/Auth/LoginCommandHandlerTests.cs`
- Create: `tests/AgendeAqui.Application.UnitTests/Auth/LoginCommandValidatorTests.cs`

**Step 1: Create User entity**

```csharp
// src/AgendeAqui.Domain/Users/User.cs
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Users;

public sealed class User : TenantEntity
{
    private User() { }

    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Role { get; private set; } = default!; // Admin, Professional, Client

    public static Result<User> Create(Guid tenantId, string email, string passwordHash, string name, string role)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(UserErrors.EmptyEmail);
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<User>(UserErrors.EmptyPassword);
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(UserErrors.EmptyName);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = name,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(user);
    }
}
```

```csharp
// src/AgendeAqui.Domain/Users/UserErrors.cs
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Users;

public static class UserErrors
{
    public static readonly Error EmptyEmail = new("User.EmptyEmail", "Email is required.");
    public static readonly Error EmptyPassword = new("User.EmptyPassword", "Password is required.");
    public static readonly Error EmptyName = new("User.EmptyName", "Name is required.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email or password.");
    public static readonly Error NotFound = new("User.NotFound", "User not found.");
}
```

**Step 2: Create IUserRepository and IJwtTokenGenerator interfaces**

```csharp
// src/AgendeAqui.Domain/Abstractions/IUserRepository.cs
using AgendeAqui.Domain.Users;

namespace AgendeAqui.Domain.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
```

```csharp
// src/AgendeAqui.Application/Auth/IJwtTokenGenerator.cs
namespace AgendeAqui.Application.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, Guid tenantId, string email, string role);
    string GenerateRefreshToken();
}
```

**Step 3: Create Login command, handler, validator, response**

```csharp
// src/AgendeAqui.Application/Auth/Login/LoginCommand.cs
using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;
```

```csharp
// src/AgendeAqui.Application/Auth/Login/LoginResponse.cs
namespace AgendeAqui.Application.Auth.Login;

public sealed record LoginResponse(string AccessToken, string RefreshToken, int ExpiresInMinutes);
```

```csharp
// src/AgendeAqui.Application/Auth/Login/LoginCommandHandler.cs
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Users;

namespace AgendeAqui.Application.Auth.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator tokenGenerator)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);

        var accessToken = tokenGenerator.GenerateToken(user.Id, user.TenantId, user.Email, user.Role);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        return new LoginResponse(accessToken, refreshToken, 60);
    }
}
```

```csharp
// src/AgendeAqui.Application/Auth/Login/LoginCommandValidator.cs
using FluentValidation;

namespace AgendeAqui.Application.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
```

**Step 4: Create JwtTokenGenerator infrastructure**

```csharp
// src/AgendeAqui.Infrastructure/Auth/JwtTokenGenerator.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AgendeAqui.Infrastructure.Auth;

internal sealed class JwtTokenGenerator(IOptions<JwtSettings> jwtSettings) : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public string GenerateToken(Guid userId, Guid tenantId, string email, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

**Step 5: Create UserRepository and UserConfiguration**

```csharp
// src/AgendeAqui.Infrastructure/Persistence/Repositories/UserRepository.cs
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await dbContext.Set<User>().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await dbContext.Set<User>().AddAsync(user, ct);
}
```

```csharp
// src/AgendeAqui.Infrastructure/Persistence/Configurations/UserConfiguration.cs
using AgendeAqui.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(u => u.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(u => new { u.TenantId, u.Email })
            .HasDatabaseName("ix_users_tenant_email")
            .IsUnique();
    }
}
```

**Step 6: Create AuthEndpoints and register in DI**

```csharp
// src/AgendeAqui.Api/Endpoints/AuthEndpoints.cs
using AgendeAqui.Application.Auth.Login;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgendeAqui.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status401Unauthorized);
        })
        .AllowAnonymous()
        .WithName("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }

    private sealed record LoginRequest(string Email, string Password);
}
```

Add to `WebApplicationExtensions.cs`:
```csharp
app.MapAuthEndpoints(); // before other endpoints
```

Add to `DependencyInjection.cs` (`AddPersistence` method):
```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Add to `DependencyInjection.cs` or `ServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
```

Add NuGet to Application project:
```bash
dotnet add src/AgendeAqui.Application/AgendeAqui.Application.csproj package BCrypt.Net-Next
```

**Step 7: Create EF Core migration**

```bash
dotnet ef migrations add AddUsersTable -p src/AgendeAqui.Infrastructure -s src/AgendeAqui.Api --output-dir Persistence/Migrations
```

**Step 8: Write unit tests**

Tests for `LoginCommandHandler`:
- `Login_WithValidCredentials_ReturnsToken`
- `Login_WithInvalidEmail_ReturnsInvalidCredentials`
- `Login_WithInvalidPassword_ReturnsInvalidCredentials`

Tests for `LoginCommandValidator`:
- `Validate_WithEmptyEmail_Fails`
- `Validate_WithEmptyPassword_Fails`
- `Validate_WithValidInput_Passes`

**Step 9: Build and run tests**

```bash
dotnet build --nologo
dotnet test --nologo --filter "FullyQualifiedName~Auth"
```

**Step 10: Commit**

```
feat: add POST /auth/login endpoint with JWT token issuance

Adds User entity, login command with BCrypt password verification,
JwtTokenGenerator, and EF Core migration for users table.
```

---

### Task 2: API Key Management Endpoints

**Why:** Dashboard needs UI to create/list/revoke API Keys. Currently no endpoints exist.

**Files:**
- Create: `src/AgendeAqui.Domain/Abstractions/IApiKeyRepository.cs`
- Create: `src/AgendeAqui.Application/ApiKeys/CreateApiKey/CreateApiKeyCommand.cs`
- Create: `src/AgendeAqui.Application/ApiKeys/CreateApiKey/CreateApiKeyCommandHandler.cs`
- Create: `src/AgendeAqui.Application/ApiKeys/CreateApiKey/CreateApiKeyCommandValidator.cs`
- Create: `src/AgendeAqui.Application/ApiKeys/ListApiKeys/ListApiKeysQuery.cs`
- Create: `src/AgendeAqui.Application/ApiKeys/ListApiKeys/ListApiKeysQueryHandler.cs`
- Create: `src/AgendeAqui.Application/ApiKeys/RevokeApiKey/RevokeApiKeyCommand.cs`
- Create: `src/AgendeAqui.Application/ApiKeys/RevokeApiKey/RevokeApiKeyCommandHandler.cs`
- Create: `src/AgendeAqui.Infrastructure/Persistence/Repositories/ApiKeyRepository.cs`
- Create: `src/AgendeAqui.Api/Endpoints/ApiKeyEndpoints.cs`
- Modify: `src/AgendeAqui.Infrastructure/DependencyInjection.cs` — register ApiKeyRepository
- Modify: `src/AgendeAqui.Api/Extensions/WebApplicationExtensions.cs` — add `app.MapApiKeyEndpoints()`
- Create: `tests/AgendeAqui.Application.UnitTests/ApiKeys/CreateApiKeyCommandHandlerTests.cs`
- Create: `tests/AgendeAqui.Application.UnitTests/ApiKeys/RevokeApiKeyCommandHandlerTests.cs`

**Endpoints:**
- `POST /api/v1/api-keys` — Create API Key (returns raw key ONCE, stores hash). RequireAdmin.
- `GET /api/v1/api-keys` — List API Keys (name, prefix, status, expiry). RequireAdmin.
- `DELETE /api/v1/api-keys/{id}` — Revoke API Key. RequireAdmin.

**Key detail:** On creation, generate random key (`sk_live_` + 32 random bytes hex), return raw key to user, store SHA256 hash only. Raw key is never stored or retrievable again.

**Commit:**
```
feat: add API Key management endpoints (create, list, revoke)
```

---

### Task 3: Add clientId Filter to ListAppointments

**Why:** Dashboard client detail page needs to show appointment history for a specific client.

**Files:**
- Modify: `src/AgendeAqui.Application/Appointments/ListAppointments/ListAppointmentsQuery.cs` — add `Guid? ClientId`
- Modify: `src/AgendeAqui.Application/Appointments/ListAppointments/ListAppointmentsQueryHandler.cs` — add `AND a.client_id = @ClientId` when present
- Modify: `src/AgendeAqui.Api/Endpoints/AppointmentEndpoints.cs` — pass `clientId` query param
- Create: `tests/AgendeAqui.Application.UnitTests/Appointments/ListAppointmentsQueryHandlerTests.cs` — test clientId filter

**Commit:**
```
feat: add clientId filter to ListAppointments query
```

---

### Task 4: Add Date Filters to ListNotifications

**Why:** Dashboard notifications page needs to filter by date range.

**Files:**
- Modify: `src/AgendeAqui.Application/Notifications/ListNotifications/ListNotificationsQuery.cs` — add `DateOnly? DateFrom`, `DateOnly? DateTo`, make `AppointmentId` optional
- Modify: `src/AgendeAqui.Application/Notifications/ListNotifications/ListNotificationsQueryHandler.cs` — add date filters, add tenant_id to SQL
- Modify: `src/AgendeAqui.Api/Endpoints/NotificationEndpoints.cs` — pass date query params

**Commit:**
```
feat: add date filters and tenant isolation to ListNotifications
```

---

### Task 5: ExternalId on Appointment

**Why:** Systems parceiros need to link their local IDs to AgendeAqui appointments.

**Files:**
- Modify: `src/AgendeAqui.Domain/Appointments/Appointment.cs` — add `ExternalId` property + `SetExternalId()` method
- Modify: `src/AgendeAqui.Infrastructure/Persistence/Configurations/AppointmentConfiguration.cs` — add `external_id` column (varchar 256, nullable), index
- Modify: `src/AgendeAqui.Application/Appointments/CreateAppointment/CreateAppointmentCommand.cs` — add `string? ExternalId`
- Modify: `src/AgendeAqui.Application/Appointments/CreateAppointment/CreateAppointmentCommandHandler.cs` — call `SetExternalId` if present
- Modify: `src/AgendeAqui.Application/Appointments/ListAppointments/ListAppointmentsQuery.cs` — add `string? ExternalId` filter
- Modify: `src/AgendeAqui.Application/Appointments/ListAppointments/ListAppointmentsQueryHandler.cs` — add `AND a.external_id = @ExternalId` when present
- Create EF migration: `AddAppointmentExternalId`
- Create: `tests/AgendeAqui.Domain.UnitTests/Appointments/AppointmentExternalIdTests.cs`

**Commit:**
```
feat: add externalId to Appointment for partner system sync
```

---

### Task 6: Idempotency Middleware

**Why:** Partner systems need to retry requests safely without creating duplicates.

**Files:**
- Create: `src/AgendeAqui.Api/Middleware/IdempotencyMiddleware.cs`
- Modify: `src/AgendeAqui.Api/Extensions/WebApplicationExtensions.cs` — register middleware
- Create: `tests/AgendeAqui.Api.UnitTests/Middleware/IdempotencyMiddlewareTests.cs`

**Implementation:** Read `X-Idempotency-Key` header on POST/PUT requests. Use HybridCache (already registered) with 24h TTL. If key exists, return cached response. If not, process normally and cache response.

**Commit:**
```
feat: add X-Idempotency-Key middleware for safe retries
```

---

### Task 7: Webhook Anti-Echo

**Why:** When a partner creates an appointment via API Key, the webhook should not fire back to that same partner.

**Files:**
- Modify: `src/AgendeAqui.Domain/Appointments/Events/AppointmentCreatedEvent.cs` — add `Guid? SourceApiKeyId`
- Modify: `src/AgendeAqui.Domain/Appointments/Appointment.cs` — pass sourceApiKeyId to Create()
- Modify: `src/AgendeAqui.Application/Appointments/CreateAppointment/CreateAppointmentCommandHandler.cs` — resolve ICurrentUser to get ApiKeyId
- Modify: `src/AgendeAqui.Application/Webhooks/Events/AppointmentCreatedWebhookHandler.cs` — include sourceApiKeyId in WebhookDeliveryIntegrationEvent
- Modify: `src/AgendeAqui.Infrastructure/Webhooks/WebhookDispatcher.cs` — skip webhooks owned by sourceApiKeyId
- Similarly for Cancelled and Rescheduled handlers
- Create: `tests/AgendeAqui.Application.UnitTests/Webhooks/WebhookAntiEchoTests.cs`

**Commit:**
```
feat: add webhook anti-echo to prevent notification loops
```

---

### Task 8: 409 Conflict Response for Invalid Transitions

**Why:** Partner systems need clear feedback when a state transition is invalid.

**Files:**
- Modify: `src/AgendeAqui.Api/Middleware/ExceptionHandlingMiddleware.cs` — map domain errors with `.IsConflict` to 409
- Modify: `src/AgendeAqui.Domain/Common/Error.cs` — add `IsConflict` property (Code ends with ".InvalidTransition")
- Modify: `src/AgendeAqui.Domain/Appointments/AppointmentErrors.cs` — rename/verify conflict error codes end with ".InvalidTransition"

**Commit:**
```
feat: map state transition errors to 409 Conflict responses
```

---

## PHASE 2: Frontend React (12 Tasks)

---

### Task 9: Scaffold React Project

**Files:**
- Create: `frontend/` directory with Vite + React + TypeScript
- Create: `frontend/package.json`
- Create: `frontend/vite.config.ts`
- Create: `frontend/tailwind.config.ts`
- Create: `frontend/tsconfig.json`
- Create: `frontend/src/App.tsx`
- Create: `frontend/src/main.tsx`
- Create: `frontend/index.html`

**Step 1: Create Vite project**

```bash
cd /c/Users/josel/source/repos/CodeProcess/AgendeAqui
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install
```

**Step 2: Install dependencies**

```bash
npm install tailwindcss @tailwindcss/vite
npm install react-router-dom @tanstack/react-query
npm install @microsoft/signalr recharts date-fns
npm install -D openapi-typescript
```

**Step 3: Configure TailwindCSS**

Add to `frontend/src/index.css`:
```css
@import "tailwindcss";
```

Add to `frontend/vite.config.ts`:
```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': 'https://localhost:5001',
      '/hubs': { target: 'https://localhost:5001', ws: true }
    }
  }
})
```

**Step 4: Verify it runs**

```bash
cd frontend && npm run dev
```

**Commit:**
```
chore: scaffold React frontend with Vite, TailwindCSS, TanStack Query
```

---

### Task 10: API Client + Auth State

**Files:**
- Create: `frontend/src/lib/api-client.ts` — fetch wrapper with JWT
- Create: `frontend/src/lib/signalr.ts` — SignalR connection
- Create: `frontend/src/store/auth-store.ts` — auth state (token, user, tenant)
- Create: `frontend/src/hooks/useAuth.ts` — login/logout/refresh
- Create: `frontend/src/hooks/useSignalR.ts` — SignalR connection hook

**Key pattern:** All API calls go through `api-client.ts` which attaches `Authorization: Bearer <token>` header automatically. On 401, redirect to `/login`.

**Commit:**
```
feat(frontend): add API client, auth store, SignalR hook
```

---

### Task 11: Layout + Navigation

**Files:**
- Create: `frontend/src/components/layout/Sidebar.tsx` — desktop sidebar
- Create: `frontend/src/components/layout/BottomNav.tsx` — mobile bottom nav
- Create: `frontend/src/components/layout/AppLayout.tsx` — responsive shell
- Create: `frontend/src/components/layout/Header.tsx` — top bar with tenant name
- Create: `frontend/src/components/ui/Toast.tsx` — notification toasts
- Modify: `frontend/src/App.tsx` — add React Router with layout

**Navigation items:** Home, Agenda, Gestao, Relatorios, Config

**Commit:**
```
feat(frontend): add responsive layout with sidebar and bottom nav
```

---

### Task 12: Login Page

**Files:**
- Create: `frontend/src/pages/login/LoginPage.tsx`
- Create: `frontend/src/pages/login/LoginForm.tsx`

**Calls:** `POST /api/v1/auth/login` → stores JWT in auth store → redirects to `/`

**Commit:**
```
feat(frontend): add login page with JWT auth
```

---

### Task 13: Home Page (Dashboard)

**Files:**
- Create: `frontend/src/pages/home/HomePage.tsx`
- Create: `frontend/src/pages/home/MetricCards.tsx` — 3 cards (total, confirmados, faturamento)
- Create: `frontend/src/pages/home/UpcomingList.tsx` — próximos agendamentos
- Create: `frontend/src/pages/home/AlertsList.tsx` — alertas

**Calls:** `GET /api/v1/appointments?dateFrom=today&dateTo=today`

**Commit:**
```
feat(frontend): add home page with daily metrics and upcoming list
```

---

### Task 14: Agenda Page (Calendar)

**Files:**
- Create: `frontend/src/pages/agenda/AgendaPage.tsx` — view switcher (day/week/month)
- Create: `frontend/src/components/agenda/DayView.tsx` — timeline vertical
- Create: `frontend/src/components/agenda/WeekView.tsx` — grade 7 colunas
- Create: `frontend/src/components/agenda/MonthView.tsx` — dots indicator
- Create: `frontend/src/components/agenda/AppointmentCard.tsx` — card with status color
- Create: `frontend/src/components/agenda/AppointmentDetail.tsx` — side panel / bottom sheet
- Create: `frontend/src/components/agenda/NewAppointmentModal.tsx` — criação de agendamento
- Create: `frontend/src/hooks/useAppointments.ts` — TanStack Query hooks
- Create: `frontend/src/lib/status-colors.ts` — color mapping for statuses

**Calls:**
- `GET /api/v1/appointments?dateFrom=X&dateTo=Y`
- `GET /api/v1/availability?professionalId=X&date=Y`
- `POST /api/v1/appointments`
- `POST /api/v1/appointments/{id}/cancel`
- `POST /api/v1/appointments/{id}/reschedule`
- SignalR `AppointmentCreated`, `AppointmentCancelled`, `AppointmentRescheduled` events for auto-refresh

**Commit:**
```
feat(frontend): add agenda page with day/week/month views and real-time updates
```

---

### Task 15: Gestao — Clientes

**Files:**
- Create: `frontend/src/pages/gestao/clientes/ClientesPage.tsx` — list with search + pagination
- Create: `frontend/src/pages/gestao/clientes/ClienteDetailPage.tsx` — detail + history + LGPD
- Create: `frontend/src/components/ui/PagedList.tsx` — reusable paginated list component
- Create: `frontend/src/components/ui/SearchInput.tsx` — debounced search input
- Create: `frontend/src/components/ui/ConfirmDialog.tsx` — destructive action confirmation
- Create: `frontend/src/hooks/useClients.ts` — TanStack Query hooks

**Calls:** `GET/POST/PUT /api/v1/clients`, `GET /api/v1/appointments?clientId=X`, `DELETE/GET /api/v1/clients/{id}/data`

**Commit:**
```
feat(frontend): add client management with LGPD actions
```

---

### Task 16: Gestao — Profissionais, Servicos, Horarios

**Files:**
- Create: `frontend/src/pages/gestao/profissionais/ProfissionaisPage.tsx`
- Create: `frontend/src/pages/gestao/servicos/ServicosPage.tsx`
- Create: `frontend/src/pages/gestao/horarios/HorariosPage.tsx`
- Create: `frontend/src/components/ui/FormModal.tsx` — reusable create/edit modal
- Create: `frontend/src/hooks/useProfessionals.ts`
- Create: `frontend/src/hooks/useServices.ts`
- Create: `frontend/src/hooks/useSchedules.ts`

**Calls:** CRUD endpoints for professionals, services, schedules.

**Commit:**
```
feat(frontend): add professional, service, and schedule management pages
```

---

### Task 17: Relatorios — Atendimentos + Faturamento

**Files:**
- Create: `frontend/src/pages/relatorios/AtendimentosPage.tsx`
- Create: `frontend/src/pages/relatorios/FaturamentoPage.tsx`
- Create: `frontend/src/components/ui/DateRangePicker.tsx`
- Create: `frontend/src/components/charts/AttendanceChart.tsx` — horizontal bars (Recharts)
- Create: `frontend/src/components/charts/RevenueChart.tsx` — bar chart by service (Recharts)
- Create: `frontend/src/hooks/useReports.ts`

**Calls:** `GET /api/v1/reports/attendance?from=X&to=Y`, `GET /api/v1/reports/revenue?from=X&to=Y`

**Commit:**
```
feat(frontend): add attendance and revenue report pages with charts
```

---

### Task 18: Config — Integracoes

**Files:**
- Create: `frontend/src/pages/config/IntegracoesPage.tsx`
- Create: `frontend/src/components/config/ApiKeyList.tsx`
- Create: `frontend/src/components/config/WebhookList.tsx`
- Create: `frontend/src/components/config/CreateApiKeyModal.tsx` — shows raw key ONCE
- Create: `frontend/src/hooks/useApiKeys.ts`
- Create: `frontend/src/hooks/useWebhooks.ts`

**Calls:** `GET/POST/DELETE /api/v1/api-keys`, `GET/POST/PUT/DELETE /api/v1/webhooks`

**Commit:**
```
feat(frontend): add integration config page (API keys + webhooks)
```

---

### Task 19: Config — Notificacoes + Conta

**Files:**
- Create: `frontend/src/pages/config/NotificacoesPage.tsx`
- Create: `frontend/src/pages/config/ContaPage.tsx`
- Create: `frontend/src/hooks/useNotifications.ts`

**Calls:** `GET /api/v1/notifications?dateFrom=X&dateTo=Y`, `GET/PUT /api/v1/tenants/{id}`

**Commit:**
```
feat(frontend): add notification history and account settings pages
```

---

### Task 20: OpenAPI Type Generation + CI

**Files:**
- Modify: `frontend/package.json` — add `generate-api` script
- Create: `.github/workflows/ci.yml` — add frontend build step
- Create: `frontend/src/api/schema.d.ts` — generated types

**Script:**
```json
{
  "scripts": {
    "generate-api": "npx openapi-typescript https://localhost:5001/openapi/v1.json -o src/api/schema.d.ts"
  }
}
```

**CI addition:**
```yaml
- name: Build frontend
  working-directory: frontend
  run: |
    npm ci
    npm run build
```

**Commit:**
```
chore: add OpenAPI type generation and frontend CI step
```

---

## PHASE 3: Polish + Integration Testing (2 Tasks)

---

### Task 21: Frontend Integration Tests

**Files:**
- Create: `frontend/src/__tests__/login.test.tsx`
- Create: `frontend/src/__tests__/agenda.test.tsx`
- Install: `vitest`, `@testing-library/react`, `msw` (Mock Service Worker)

**Commit:**
```
test(frontend): add integration tests for login and agenda flows
```

---

### Task 22: Update Architecture Map + CLAUDE.md

**Files:**
- Modify: `memory/architecture-complete-map.md` — add Phase 8 (Dashboard) section
- Modify: `CLAUDE.md` — add frontend commands section

**Add to CLAUDE.md:**
```markdown
## Frontend Commands

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
```

**Commit:**
```
docs: update architecture map and CLAUDE.md with frontend commands
```

---

## Summary

| Phase | Tasks | Scope |
|-------|-------|-------|
| Phase 1 | Tasks 1-8 | Backend API gaps (auth, api-keys, filters, sync) |
| Phase 2 | Tasks 9-20 | Frontend React (scaffold, all pages, CI) |
| Phase 3 | Tasks 21-22 | Tests + docs |

**Total: 22 tasks**

**Dependencies:**
- Task 1 (login) must complete before Task 12 (login page)
- Task 2 (api-keys) must complete before Task 18 (integrations page)
- Task 3 (clientId filter) must complete before Task 15 (client detail)
- Task 4 (notification filters) must complete before Task 19 (notifications page)
- Tasks 5-8 (sync) are independent of frontend and can be done in parallel
- Task 9 (scaffold) must complete before all other frontend tasks (10-20)
- Task 10 (api client) must complete before all page tasks (11-20)
- Task 11 (layout) must complete before all page tasks (12-20)

**Recommended execution order:**
1. Tasks 1-4 (backend blockers — sequential)
2. Task 9 (scaffold — start frontend)
3. Tasks 10-11 (infra frontend — sequential)
4. Task 12 (login page)
5. Tasks 13-19 (pages — can be parallelized with subagents)
6. Tasks 5-8 (sync — can be done in parallel with frontend)
7. Tasks 20-22 (CI, tests, docs — final)
