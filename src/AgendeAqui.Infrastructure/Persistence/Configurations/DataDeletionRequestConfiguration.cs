using AgendeAqui.Domain.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class DataDeletionRequestConfiguration : IEntityTypeConfiguration<DataDeletionRequest>
{
    public void Configure(EntityTypeBuilder<DataDeletionRequest> builder)
    {
        builder.ToTable("data_deletion_requests");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(d => d.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(d => d.RequestedAt).HasColumnName("requested_at").IsRequired();
        builder.Property(d => d.CompletedAt).HasColumnName("completed_at");
        builder.Property(d => d.Status).HasColumnName("status").HasMaxLength(20).IsRequired()
            .HasConversion<string>();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(d => new { d.TenantId, d.ClientId })
            .HasDatabaseName("ix_data_deletion_requests_tenant_client");
    }
}
