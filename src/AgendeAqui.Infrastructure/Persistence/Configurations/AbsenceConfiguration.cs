using AgendeAqui.Domain.Schedules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class AbsenceConfiguration : IEntityTypeConfiguration<Absence>
{
    public void Configure(EntityTypeBuilder<Absence> builder)
    {
        builder.ToTable("absences");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(a => a.ProfessionalId).HasColumnName("professional_id").IsRequired();
        builder.Property(a => a.Date).HasColumnName("date").IsRequired();
        builder.Property(a => a.StartTime).HasColumnName("start_time");
        builder.Property(a => a.EndTime).HasColumnName("end_time");
        builder.Property(a => a.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => new { a.TenantId, a.ProfessionalId, a.Date })
            .HasDatabaseName("ix_absences_tenant_professional_date");
    }
}
