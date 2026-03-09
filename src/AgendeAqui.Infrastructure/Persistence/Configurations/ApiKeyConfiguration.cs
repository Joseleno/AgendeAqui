using AgendeAqui.Domain.ApiKeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id)
            .HasColumnName("id");

        builder.Property(k => k.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(k => k.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(k => k.KeyHash)
            .HasColumnName("key_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(k => k.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(k => k.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(k => k.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(k => k.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(k => k.KeyHash)
            .IsUnique()
            .HasDatabaseName("ix_api_keys_key_hash");

        builder.HasIndex(k => k.TenantId)
            .HasDatabaseName("ix_api_keys_tenant_id");

        builder.HasIndex(k => new { k.TenantId, k.IsActive })
            .HasFilter("is_active = true")
            .HasDatabaseName("ix_api_keys_tenant_active");
    }
}
