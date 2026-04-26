using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .HasConversion(
                e => e.Value,
                s => Email.Hydrate(s))
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .HasConversion(
                ph => ph.Value,
                s => PhoneNumber.Hydrate(s))
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasMaxLength(Client.MaxNotesLength);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_clients_tenant_id");

        builder.HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique()
            .HasDatabaseName("ix_clients_tenant_email");
    }
}
