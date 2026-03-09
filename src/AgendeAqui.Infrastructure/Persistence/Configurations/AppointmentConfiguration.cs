using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(a => a.ProfessionalId)
            .HasColumnName("professional_id")
            .IsRequired();

        builder.Property(a => a.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(a => a.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(a => a.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.OwnsOne(a => a.TimeSlot, ts =>
        {
            ts.Property(t => t.Start)
                .HasColumnName("start_time")
                .IsRequired();

            ts.Property(t => t.End)
                .HasColumnName("end_time")
                .IsRequired();
        });

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasConversion(
                s => s.Name,
                n => AppointmentStatus.Hydrate(n))
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(a => a.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(a => new { a.TenantId, a.Date })
            .HasDatabaseName("ix_appointments_tenant_date");

        builder.HasIndex(a => new { a.ProfessionalId, a.Date })
            .HasDatabaseName("ix_appointments_professional_date");

        builder.HasIndex(a => new { a.TenantId, a.ExternalId })
            .HasDatabaseName("ix_appointments_tenant_external_id")
            .HasFilter("external_id IS NOT NULL");

        builder.Ignore(a => a.DomainEvents);
    }
}
