using AgendeAqui.Domain.Professionals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalServiceConfiguration : IEntityTypeConfiguration<ProfessionalService>
{
    public void Configure(EntityTypeBuilder<ProfessionalService> builder)
    {
        builder.ToTable("professional_services");

        builder.HasKey(ps => new { ps.ProfessionalId, ps.ServiceId });

        builder.Property(ps => ps.Id).HasColumnName("id");
        builder.Property(ps => ps.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(ps => ps.ProfessionalId).HasColumnName("professional_id").IsRequired();
        builder.Property(ps => ps.ServiceId).HasColumnName("service_id").IsRequired();
        builder.Property(ps => ps.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(ps => ps.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(ps => new { ps.TenantId, ps.ProfessionalId })
            .HasDatabaseName("ix_professional_services_tenant_professional");

        builder.HasIndex(ps => new { ps.TenantId, ps.ServiceId })
            .HasDatabaseName("ix_professional_services_tenant_service");
    }
}
