using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.Services;
using AgendeAqui.Domain.Tenants;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
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
