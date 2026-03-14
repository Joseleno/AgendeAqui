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

        // Seed second tenant: Mental Health Clinic
        var mentalTenantId = Guid.Parse("c1d2e3f4-a5b6-7890-cdef-123456789abc");
        await using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = BuildMentalHealthSeedSql(mentalTenantId, now);
        await cmd2.ExecuteNonQueryAsync();

        app.Logger.LogInformation("Mental health clinic seed created. Tenant ID: {TenantId}", mentalTenantId);
        app.Logger.LogInformation("Login: admin@menteviva.com / Admin123!");
        app.Logger.LogInformation("Login: dra.patricia@menteviva.com / Admin123! (Psicóloga)");
        app.Logger.LogInformation("Login: joana.paciente@email.com / Admin123! (Paciente)");
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
        var profUserId = Guid.Parse("dddd2222-2222-2222-2222-222222222222");
        var clientUserId = Guid.Parse("dddd3333-3333-3333-3333-333333333333");

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

        -- Users (admin, professional, client)
        INSERT INTO users (id, tenant_id, email, password_hash, name, role, professional_id, client_id, created_at) VALUES
        ('{userId}', '{tenantId}', 'admin@beleza.com', '{passwordHash}', 'Administrador', 'Admin', null, null, '{ts}'),
        ('{profUserId}', '{tenantId}', 'ana@beleza.com', '{passwordHash}', 'Ana Silva', 'Professional', '{prof1Id}', null, '{ts}'),
        ('{clientUserId}', '{tenantId}', 'maria@email.com', '{passwordHash}', 'Maria Santos', 'Client', null, '{cli1Id}', '{ts}');

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

    private static string BuildMentalHealthSeedSql(Guid tenantId, DateTime now)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        // Professionals (5 mental health specialists)
        var psico1Id = Guid.Parse("55551111-1111-1111-1111-111111111111"); // Dra. Patricia - Psicóloga Clínica
        var psico2Id = Guid.Parse("55552222-2222-2222-2222-222222222222"); // Dr. Ricardo - Psiquiatra
        var psico3Id = Guid.Parse("55553333-3333-3333-3333-333333333333"); // Dra. Camila - Neuropsicóloga
        var psico4Id = Guid.Parse("55554444-4444-4444-4444-444444444444"); // Dr. Fernando - Psicólogo TCC
        var psico5Id = Guid.Parse("55555555-5555-5555-5555-555555555555"); // Dra. Lucia - Psiquiatra Infantil

        // Services
        var svcTerapia = Guid.Parse("cccc1111-1111-1111-1111-111111111111");
        var svcConsultaPsiq = Guid.Parse("cccc2222-2222-2222-2222-222222222222");
        var svcAvaliacao = Guid.Parse("cccc3333-3333-3333-3333-333333333333");
        var svcTerapiaCasal = Guid.Parse("cccc4444-4444-4444-4444-444444444444");
        var svcRetorno = Guid.Parse("cccc5555-5555-5555-5555-555555555555");
        var svcNeuropsico = Guid.Parse("cccc6666-6666-6666-6666-666666666666");
        var svcTerapiaInfantil = Guid.Parse("cccc7777-7777-7777-7777-777777777777");

        // Clients (8 patients)
        var pac1Id = Guid.Parse("eeee1111-1111-1111-1111-111111111111");
        var pac2Id = Guid.Parse("eeee2222-2222-2222-2222-222222222222");
        var pac3Id = Guid.Parse("eeee3333-3333-3333-3333-333333333333");
        var pac4Id = Guid.Parse("eeee4444-4444-4444-4444-444444444444");
        var pac5Id = Guid.Parse("eeee5555-5555-5555-5555-555555555555");
        var pac6Id = Guid.Parse("eeee6666-6666-6666-6666-666666666666");
        var pac7Id = Guid.Parse("eeee7777-7777-7777-7777-777777777777");
        var pac8Id = Guid.Parse("eeee8888-8888-8888-8888-888888888888");

        // Users
        var adminUserId = Guid.Parse("ffff1111-1111-1111-1111-111111111111");
        var profUserId = Guid.Parse("ffff2222-2222-2222-2222-222222222222");
        var clientUserId = Guid.Parse("ffff3333-3333-3333-3333-333333333333");

        var todayDate = DateOnly.FromDateTime(now);
        var today = todayDate.ToString("yyyy-MM-dd");
        var tomorrow = todayDate.AddDays(1).ToString("yyyy-MM-dd");
        var dayAfter = todayDate.AddDays(2).ToString("yyyy-MM-dd");
        var yesterday = todayDate.AddDays(-1).ToString("yyyy-MM-dd");
        var twoDaysAgo = todayDate.AddDays(-2).ToString("yyyy-MM-dd");
        var threeDaysAgo = todayDate.AddDays(-3).ToString("yyyy-MM-dd");

        var ts = now.ToString("yyyy-MM-dd HH:mm:ss");

        return $"""
        -- Tenant: Clinica de Saude Mental
        INSERT INTO tenants (id, name, slug, status, plan, created_at)
        VALUES ('{tenantId}', 'Mente Viva - Saude Mental', 'mente-viva', 1, 3, '{ts}');

        -- Users (admin, professional, client)
        INSERT INTO users (id, tenant_id, email, password_hash, name, role, professional_id, client_id, created_at) VALUES
        ('{adminUserId}', '{tenantId}', 'admin@menteviva.com', '{passwordHash}', 'Dr. Roberto Mendes', 'Admin', null, null, '{ts}'),
        ('{profUserId}', '{tenantId}', 'dra.patricia@menteviva.com', '{passwordHash}', 'Dra. Patricia Almeida', 'Professional', '{psico1Id}', null, '{ts}'),
        ('{clientUserId}', '{tenantId}', 'joana.paciente@email.com', '{passwordHash}', 'Joana Oliveira', 'Client', null, '{pac1Id}', '{ts}');

        -- Professionals (5 specialists)
        INSERT INTO professionals (id, tenant_id, name, email, phone, is_active, specialty, created_at) VALUES
        ('{psico1Id}', '{tenantId}', 'Dra. Patricia Almeida', 'dra.patricia@menteviva.com', '+5511997770001', true, 'Psicologia Clinica', '{ts}'),
        ('{psico2Id}', '{tenantId}', 'Dr. Ricardo Souza', 'dr.ricardo@menteviva.com', '+5511997770002', true, 'Psiquiatria', '{ts}'),
        ('{psico3Id}', '{tenantId}', 'Dra. Camila Torres', 'dra.camila@menteviva.com', '+5511997770003', true, 'Neuropsicologia', '{ts}'),
        ('{psico4Id}', '{tenantId}', 'Dr. Fernando Lima', 'dr.fernando@menteviva.com', '+5511997770004', true, 'Psicologia - TCC', '{ts}'),
        ('{psico5Id}', '{tenantId}', 'Dra. Lucia Ramos', 'dra.lucia@menteviva.com', '+5511997770005', true, 'Psiquiatria Infantil', '{ts}');

        -- Services (7 types)
        INSERT INTO services (id, tenant_id, name, duration, price, is_active, created_at) VALUES
        ('{svcTerapia}', '{tenantId}', 'Sessao de Psicoterapia', '00:50:00', 250.00, true, '{ts}'),
        ('{svcConsultaPsiq}', '{tenantId}', 'Consulta Psiquiatrica', '00:30:00', 400.00, true, '{ts}'),
        ('{svcAvaliacao}', '{tenantId}', 'Avaliacao Psicologica', '01:30:00', 500.00, true, '{ts}'),
        ('{svcTerapiaCasal}', '{tenantId}', 'Terapia de Casal', '01:15:00', 350.00, true, '{ts}'),
        ('{svcRetorno}', '{tenantId}', 'Retorno Psiquiatrico', '00:20:00', 300.00, true, '{ts}'),
        ('{svcNeuropsico}', '{tenantId}', 'Avaliacao Neuropsicologica', '02:00:00', 800.00, true, '{ts}'),
        ('{svcTerapiaInfantil}', '{tenantId}', 'Terapia Infantil', '00:45:00', 280.00, true, '{ts}');

        -- Professional-Service links
        INSERT INTO professional_services (id, professional_id, service_id, tenant_id, created_at) VALUES
        ('{Guid.NewGuid()}', '{psico1Id}', '{svcTerapia}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico1Id}', '{svcAvaliacao}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico1Id}', '{svcTerapiaCasal}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico2Id}', '{svcConsultaPsiq}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico2Id}', '{svcRetorno}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico3Id}', '{svcNeuropsico}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico3Id}', '{svcAvaliacao}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico4Id}', '{svcTerapia}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico4Id}', '{svcTerapiaCasal}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico5Id}', '{svcConsultaPsiq}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico5Id}', '{svcRetorno}', '{tenantId}', '{ts}'),
        ('{Guid.NewGuid()}', '{psico5Id}', '{svcTerapiaInfantil}', '{tenantId}', '{ts}');

        -- Clients (8 patients)
        INSERT INTO clients (id, tenant_id, name, email, phone, created_at) VALUES
        ('{pac1Id}', '{tenantId}', 'Joana Oliveira', 'joana.paciente@email.com', '+5511966660001', '{ts}'),
        ('{pac2Id}', '{tenantId}', 'Carlos Mendonca', 'carlos.m@email.com', '+5511966660002', '{ts}'),
        ('{pac3Id}', '{tenantId}', 'Ana Paula Costa', 'anapaula@email.com', '+5511966660003', '{ts}'),
        ('{pac4Id}', '{tenantId}', 'Roberto Dias', 'roberto.dias@email.com', '+5511966660004', '{ts}'),
        ('{pac5Id}', '{tenantId}', 'Mariana Silva', 'mariana.s@email.com', '+5511966660005', '{ts}'),
        ('{pac6Id}', '{tenantId}', 'Thiago Pereira', 'thiago.p@email.com', '+5511966660006', '{ts}'),
        ('{pac7Id}', '{tenantId}', 'Beatriz Santos', 'beatriz.s@email.com', '+5511966660007', '{ts}'),
        ('{pac8Id}', '{tenantId}', 'Rafael Gomes', 'rafael.g@email.com', '+5511966660008', '{ts}');

        -- Schedules
        INSERT INTO schedules (id, tenant_id, professional_id, day_of_week, start_time, end_time, slot_duration, is_active, created_at) VALUES
        -- Dra. Patricia: Mon-Fri 08:00-18:00, sessoes de 50min (1h slot)
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', 1, '08:00:00', '18:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', 2, '08:00:00', '18:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', 3, '08:00:00', '18:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', 4, '08:00:00', '18:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', 5, '08:00:00', '18:00:00', '01:00:00', true, '{ts}'),
        -- Dr. Ricardo (Psiquiatra): Mon-Thu 09:00-17:00, consultas de 30min
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', 1, '09:00:00', '17:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', 2, '09:00:00', '17:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', 3, '09:00:00', '17:00:00', '00:30:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', 4, '09:00:00', '17:00:00', '00:30:00', true, '{ts}'),
        -- Dra. Camila (Neuropsico): Tue-Fri 08:00-16:00, sessoes de 2h
        ('{Guid.NewGuid()}', '{tenantId}', '{psico3Id}', 2, '08:00:00', '16:00:00', '02:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico3Id}', 3, '08:00:00', '16:00:00', '02:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico3Id}', 4, '08:00:00', '16:00:00', '02:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico3Id}', 5, '08:00:00', '16:00:00', '02:00:00', true, '{ts}'),
        -- Dr. Fernando (TCC): Mon-Fri 10:00-20:00, sessoes de 1h
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', 1, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', 2, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', 3, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', 4, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', 5, '10:00:00', '20:00:00', '01:00:00', true, '{ts}'),
        -- Dra. Lucia (Psiq Infantil): Mon,Wed,Fri 08:00-14:00, consultas de 45min
        ('{Guid.NewGuid()}', '{tenantId}', '{psico5Id}', 1, '08:00:00', '14:00:00', '00:45:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico5Id}', 3, '08:00:00', '14:00:00', '00:45:00', true, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico5Id}', 5, '08:00:00', '14:00:00', '00:45:00', true, '{ts}');

        -- Absences (null start_time/end_time = full day)
        INSERT INTO absences (id, tenant_id, professional_id, date, start_time, end_time, reason, created_at) VALUES
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', '{tomorrow}', null, null, 'Congresso de Psiquiatria', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{dayAfter}', '14:00:00', '18:00:00', 'Supervisao clinica', '{ts}');

        -- Appointments (rich mix across days)
        INSERT INTO appointments (id, tenant_id, professional_id, service_id, client_id, date, start_time, end_time, status, notes, created_at) VALUES
        -- Today: 8 appointments
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapia}', '{pac1Id}', '{today}', '08:00:00', '08:50:00', 'Confirmed', 'Sessao semanal - ansiedade', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapia}', '{pac3Id}', '{today}', '09:00:00', '09:50:00', 'Scheduled', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapiaCasal}', '{pac5Id}', '{today}', '10:00:00', '11:15:00', 'Confirmed', 'Terapia de casal - 3a sessao', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', '{svcConsultaPsiq}', '{pac2Id}', '{today}', '09:00:00', '09:30:00', 'Confirmed', 'Ajuste de medicacao', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', '{svcRetorno}', '{pac4Id}', '{today}', '10:00:00', '10:20:00', 'Scheduled', 'Retorno - verificar efeitos', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico3Id}', '{svcNeuropsico}', '{pac6Id}', '{today}', '08:00:00', '10:00:00', 'Confirmed', 'Avaliacao TDAH', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', '{svcTerapia}', '{pac7Id}', '{today}', '10:00:00', '10:50:00', 'Confirmed', 'TCC - fobia social', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico5Id}', '{svcTerapiaInfantil}', '{pac8Id}', '{today}', '08:00:00', '08:45:00', 'Scheduled', 'Ludoterapia - 5 anos', '{ts}'),

        -- Tomorrow: 5 appointments
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapia}', '{pac2Id}', '{tomorrow}', '08:00:00', '08:50:00', 'Scheduled', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcAvaliacao}', '{pac6Id}', '{tomorrow}', '10:00:00', '11:30:00', 'Scheduled', 'Avaliacao para laudo', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', '{svcTerapia}', '{pac1Id}', '{tomorrow}', '14:00:00', '14:50:00', 'Scheduled', 'TCC - exposicao gradual', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', '{svcTerapiaCasal}', '{pac3Id}', '{tomorrow}', '16:00:00', '17:15:00', 'Scheduled', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico5Id}', '{svcTerapiaInfantil}', '{pac8Id}', '{tomorrow}', '08:00:00', '08:45:00', 'Scheduled', 'Continuacao ludoterapia', '{ts}'),

        -- Yesterday: completed + cancelled + no-show
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapia}', '{pac1Id}', '{yesterday}', '08:00:00', '08:50:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapia}', '{pac5Id}', '{yesterday}', '09:00:00', '09:50:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', '{svcConsultaPsiq}', '{pac2Id}', '{yesterday}', '09:00:00', '09:30:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', '{svcRetorno}', '{pac7Id}', '{yesterday}', '10:00:00', '10:20:00', 'Cancelled', 'Paciente desmarcou', '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', '{svcTerapia}', '{pac3Id}', '{yesterday}', '14:00:00', '14:50:00', 'NoShow', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico3Id}', '{svcNeuropsico}', '{pac4Id}', '{yesterday}', '08:00:00', '10:00:00', 'Completed', null, '{ts}'),

        -- 2 days ago: all completed
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapia}', '{pac3Id}', '{twoDaysAgo}', '08:00:00', '08:50:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', '{svcConsultaPsiq}', '{pac1Id}', '{twoDaysAgo}', '09:00:00', '09:30:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico4Id}', '{svcTerapia}', '{pac7Id}', '{twoDaysAgo}', '10:00:00', '10:50:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico5Id}', '{svcTerapiaInfantil}', '{pac8Id}', '{twoDaysAgo}', '08:00:00', '08:45:00', 'Completed', null, '{ts}'),

        -- 3 days ago
        ('{Guid.NewGuid()}', '{tenantId}', '{psico1Id}', '{svcTerapiaCasal}', '{pac5Id}', '{threeDaysAgo}', '10:00:00', '11:15:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico2Id}', '{svcConsultaPsiq}', '{pac6Id}', '{threeDaysAgo}', '09:00:00', '09:30:00', 'Completed', null, '{ts}'),
        ('{Guid.NewGuid()}', '{tenantId}', '{psico3Id}', '{svcAvaliacao}', '{pac2Id}', '{threeDaysAgo}', '08:00:00', '09:30:00', 'Completed', null, '{ts}');
        """;
    }
}
