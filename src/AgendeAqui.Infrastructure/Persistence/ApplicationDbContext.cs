using System.Linq.Expressions;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ClinicalNotes;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.InAppNotifications;
using AgendeAqui.Domain.Payments;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Schedules;
using ProfessionalServiceEntity = AgendeAqui.Domain.Professionals.ProfessionalService;
using AgendeAqui.Domain.Services;
using AgendeAqui.Domain.Tenants;
using AgendeAqui.Domain.Teams;
using AgendeAqui.Domain.Webhooks;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;
    private readonly ITenantProvider _tenantProvider;

    // Exposed as property so EF Core can parameterize it in query filters.
    // EF Core re-evaluates DbContext member access on each query execution.
    public Guid CurrentTenantId => _tenantProvider.GetTenantId();

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<ProfessionalServiceEntity> ProfessionalServices => Set<ProfessionalServiceEntity>();
    public DbSet<Absence> Absences => Set<Absence>();
    public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();
    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

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

            // e => e.TenantId == EF.Property<Guid>(this, "CurrentTenantId")
            // EF Core parameterizes DbContext member access, ensuring the filter
            // uses the current tenant ID for each query execution.
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProperty = Expression.Property(parameter, nameof(TenantEntity.TenantId));
            var currentTenantId = Expression.Property(
                Expression.Constant(this),
                nameof(CurrentTenantId));

            var filter = Expression.Lambda(
                Expression.Equal(tenantIdProperty, currentTenantId),
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
