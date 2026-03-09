using System.Linq.Expressions;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.Services;
using AgendeAqui.Domain.Tenants;
using AgendeAqui.Domain.Webhooks;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;
    private readonly ITenantProvider _tenantProvider;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IPublisher publisher,
        ITenantProvider tenantProvider)
        : base(options)
    {
        _publisher = publisher;
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplyGlobalQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");

            var tenantIdProperty = Expression.Property(parameter, nameof(TenantEntity.TenantId));

            var tenantProviderExpr = Expression.Constant(_tenantProvider);
            var getTenantIdCall = Expression.Call(
                tenantProviderExpr,
                typeof(ITenantProvider).GetMethod(nameof(ITenantProvider.GetTenantId))!);

            var filter = Expression.Lambda(
                Expression.Equal(tenantIdProperty, getTenantIdCall),
                parameter);

            entityType.SetQueryFilter(filter);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);

        return result;
    }

    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct) =>
        await SaveChangesAsync(ct);
}
