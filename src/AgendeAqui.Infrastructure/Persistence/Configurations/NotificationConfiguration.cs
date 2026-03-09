using AgendeAqui.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(n => n.AppointmentId).HasColumnName("appointment_id").IsRequired();
        builder.Property(n => n.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.Recipient).HasColumnName("recipient").HasMaxLength(50).IsRequired();
        builder.Property(n => n.TemplateName).HasColumnName("template_name").HasMaxLength(100).IsRequired();
        builder.Property(n => n.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.SentAt).HasColumnName("sent_at");
        builder.Property(n => n.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(n => new { n.TenantId, n.AppointmentId })
            .HasDatabaseName("ix_notifications_tenant_appointment");

        builder.HasIndex(n => new { n.TenantId, n.CreatedAt })
            .HasDatabaseName("ix_notifications_tenant_created");
    }
}
