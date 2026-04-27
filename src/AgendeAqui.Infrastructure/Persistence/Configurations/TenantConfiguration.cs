using AgendeAqui.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.ConnectionString)
            .HasColumnName("connection_string")
            .HasMaxLength(500);

        builder.Property(t => t.CustomDomain)
            .HasColumnName("custom_domain")
            .HasMaxLength(253);

        builder.Property(t => t.TrialEndsAt)
            .HasColumnName("trial_ends_at");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Plan)
            .HasColumnName("plan")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.Labels)
            .HasColumnName("labels")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                s => JsonSerializer.Deserialize<TenantLabels>(s, JsonOptions) ?? TenantLabels.Default())
            .IsRequired();

        builder.Property(t => t.Features)
            .HasColumnName("features_enabled")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                s => JsonSerializer.Deserialize<TenantFeatures>(s, JsonOptions) ?? TenantFeatures.Default())
            .IsRequired();

        builder.Property(t => t.Theme)
            .HasColumnName("theme")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                s => JsonSerializer.Deserialize<TenantTheme>(s, JsonOptions) ?? TenantTheme.Default())
            .IsRequired();

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tenants_slug");

        builder.HasIndex(t => t.CustomDomain)
            .IsUnique()
            .HasFilter("custom_domain IS NOT NULL")
            .HasDatabaseName("ix_tenants_custom_domain");
    }
}
