using AgendeAqui.Domain.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("webhook_deliveries");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(d => d.WebhookId).HasColumnName("webhook_id").IsRequired();
        builder.Property(d => d.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(d => d.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(d => d.ResponseCode).HasColumnName("response_code");
        builder.Property(d => d.Attempt).HasColumnName("attempt").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<Webhook>()
            .WithMany()
            .HasForeignKey(d => d.WebhookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.WebhookId, d.CreatedAt })
            .HasDatabaseName("ix_webhook_deliveries_webhook_created");
    }
}
