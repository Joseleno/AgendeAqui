using AgendeAqui.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AgendeAqui.Api;

internal static class SeedData
{
    public static async Task InitializeAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        // Check if seed already ran
        if (await db.Tenants.IgnoreQueryFilters().AnyAsync())
            return;

        var tenantId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var now = DateTime.UtcNow;

        // Use raw SQL to bypass tenant query filters and private constructors
        var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = BuildSeedSql(tenantId, now);
        await cmd.ExecuteNonQueryAsync();

        app.Logger.LogInformation("Seed data created. Tenant ID: {TenantId}", tenantId);
        app.Logger.LogInformation("Login: admin@beleza.com / Admin123!");
    }

    private static string BuildSeedSql(Guid tenantId, DateTime now)
    {
        // BCrypt hash for "Admin123!"
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        // Fixed IDs for referencing
        var prof1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var prof2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var prof3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var svc1Id = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
        var svc2Id = Guid.Parse("aaaa2222-2222-2222-2222-222222222222");
        var svc3Id = Guid.Parse("aaaa3333-3333-3333-3333-333333333333");
        var svc4Id = Guid.Parse("aaaa4444-4444-4444-4444-444444444444");

        var cli1Id = Guid.Parse("bbbb1111-1111-1111-1111-111111111111");
        var cli2Id = Guid.Parse("bbbb2222-2222-2222-2222-222222222222");
        var cli3Id = Guid.Parse("bbbb3333-3333-3333-3333-333333333333");
        var cli4Id = Guid.Parse("bbbb4444-4444-4444-4444-444444444444");
        var cli5Id = Guid.Parse("bbbb5555-5555-5555-5555-555555555555");
        var cli6Id = Guid.Parse("bbbb6666-6666-6666-6666-666666666666");

        var userId = Guid.Parse("dddd1111-1111-1111-1111-111111111111");

        var todayDate = DateOnly.FromDateTime(now);
        var today = todayDate.ToString("yyyy-MM-dd");
        var tomorrow = todayDate.AddDays(1).ToString("yyyy-MM-dd");
        var yesterday = todayDate.AddDays(-1).ToString("yyyy-MM-dd");
        var twoDaysAgo = todayDate.AddDays(-2).ToString("yyyy-MM-dd");

        var ts = now.ToString("yyyy-MM-dd HH:mm:ss");

        return $"""
        -- Tenant
        INSERT INTO tenants (id, name, slug, status, plan, created_at)
        VALUES ('{tenantId}', 'Beleza & Cia', 'beleza-e-cia', 1, 3, '{ts}');

        -- User (admin)
        INSERT INTO users (id, tenant_id, email, password_hash, name, role, created_at)
        VALUES ('{userId}', '{tenantId}', 'admin@beleza.com', '{passwordHash}', 'Administrador', 'Admin', '{ts}');

        -- Professionals
        INSERT INTO professionals (id, tenant_id, name, email, phone, is_active, created_at) VALUES
        ('{prof1Id}', '{tenantId}', 'Ana Silva', 'ana@beleza.com', '+5511999990001', true, '{ts}'),
        ('{prof2Id}', '{tenantId}', 'Carlos Oliveira', 'carlos@beleza.com', '+5511999990002', true, '{ts}'),
        ('{prof3Id}', '{tenantId}', 'Juliana Costa', 'juliana@beleza.com', '+5511999990003', true, '{ts}');

        -- Services
        INSERT INTO services (id, tenant_id, name, duration, price, is_active, created_at) VALUES
        ('{svc1Id}', '{tenantId}', 'Corte Feminino', '00:45:00', 80.00, true, '{ts}'),
        ('{svc2Id}', '{tenantId}', 'Corte Masculino', '00:30:00', 50.00, true, '{ts}'),
        ('{svc3Id}', '{tenantId}', 'Manicure', '01:00:00', 45.00, true, '{ts}'),
        ('{svc4Id}', '{tenantId}', 'Escova Progressiva', '02:00:00', 200.00, true, '{ts}');

        -- Clients
        INSERT INTO clients (id, tenant_id, name, email, phone, created_at) VALUES
        ('{cli1Id}', '{tenantId}', 'Maria Santos', 'maria@email.com', '+5511988880001', '{ts}'),
        ('{cli2Id}', '{tenantId}', 'Joao Lima', 'joao@email.com', '+5511988880002', '{ts}'),
        ('{cli3Id}', '{tenantId}', 'Fernanda Rocha', 'fernanda@email.com', '+5511988880003', '{ts}'),
        ('{cli4Id}', '{tenantId}', 'Pedro Almeida', 'pedro@email.com', '+5511988880004', '{ts}'),
        ('{cli5Id}', '{tenantId}', 'Camila Ferreira', 'camila@email.com', '+5511988880005', '{ts}'),
        ('{cli6Id}', '{tenantId}', 'Lucas Barbosa', 'lucas@email.com', '+5511988880006', '{ts}');

        -- Schedules (Mon-Sat for each professional)
        INSERT INTO schedules (id, tenant_id, professional_id, day_of_week, start_time, end_time, slot_duration, is_active, created_at) VALUES
        -- Ana: Mon-Fri 08:00-18:00, slots 30min
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', 1, '08:00:00', '18:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', 2, '08:00:00', '18:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', 3, '08:00:00', '18:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', 4, '08:00:00', '18:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', 5, '08:00:00', '18:00:00', '00:30:00', true, '{ts}'),
        -- Carlos: Mon-Sat 09:00-19:00, slots 30min
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', 1, '09:00:00', '19:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', 2, '09:00:00', '19:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', 3, '09:00:00', '19:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', 4, '09:00:00', '19:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', 5, '09:00:00', '19:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', 6, '09:00:00', '14:00:00', '00:30:00', true, '{ts}'),
        -- Juliana: Tue-Sat 10:00-20:00, slots 1h
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', 2, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', 3, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', 4, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', 5, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', 6, '10:00:00', '16:00:00', '01:00:00', true, '{ts}');

        -- Appointments (mix of statuses across different days)
        INSERT INTO appointments (id, tenant_id, professional_id, service_id, client_id, date, start_time, end_time, status, notes, created_at) VALUES
        -- Today: 5 appointments
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', '{svc1Id}', '{cli1Id}', '{today}', '08:00:00', '08:45:00', 'Confirmed', 'Cliente frequente', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', '{svc1Id}', '{cli3Id}', '{today}', '09:00:00', '09:45:00', 'Scheduled', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', '{svc2Id}', '{cli2Id}', '{today}', '10:00:00', '10:30:00', 'Confirmed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', '{svc2Id}', '{cli4Id}', '{today}', '14:00:00', '14:30:00', 'Scheduled', 'Primeira visita', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', '{svc3Id}', '{cli5Id}', '{today}', '10:00:00', '11:00:00', 'Confirmed', null, '{ts}'),

        -- Tomorrow: 3 appointments
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', '{svc4Id}', '{cli6Id}', '{tomorrow}', '08:00:00', '10:00:00', 'Scheduled', 'Trazer referencia de cor', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', '{svc2Id}', '{cli2Id}', '{tomorrow}', '09:00:00', '09:30:00', 'Scheduled', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', '{svc3Id}', '{cli1Id}', '{tomorrow}', '14:00:00', '15:00:00', 'Scheduled', null, '{ts}'),

        -- Yesterday: completed, cancelled, no-show
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', '{svc1Id}', '{cli1Id}', '{yesterday}', '08:00:00', '08:45:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', '{svc2Id}', '{cli4Id}', '{yesterday}', '10:00:00', '10:30:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', '{svc1Id}', '{cli3Id}', '{yesterday}', '14:00:00', '14:45:00', 'Cancelled', 'Cliente desmarcou', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', '{svc3Id}', '{cli5Id}', '{yesterday}', '10:00:00', '11:00:00', 'NoShow', null, '{ts}'),

        -- 2 days ago: all completed
        ('{Guid.NewGuid()}', '{tenantId}', '{prof1Id}', '{svc1Id}', '{cli2Id}', '{twoDaysAgo}', '08:00:00', '08:45:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof2Id}', '{svc2Id}', '{cli6Id}', '{twoDaysAgo}', '09:00:00', '09:30:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{prof3Id}', '{svc4Id}', '{cli3Id}', '{twoDaysAgo}', '10:00:00', '12:00:00', 'Completed', null, '{ts}');
        """;
    }
}
