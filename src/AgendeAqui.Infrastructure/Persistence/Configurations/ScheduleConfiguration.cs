using AgendeAqui.Domain.Schedules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeAqui.Infrastructure.Persistence.Configurations;

internal sealed class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(s => s.ProfessionalId)
            .HasColumnName("professional_id")
            .IsRequired();

        builder.Property(s => s.DayOfWeek)
            .HasColumnName("day_of_week")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(s => s.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(s => s.SlotDuration)
            .HasColumnName("slot_duration")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.OwnsMany(s => s.Breaks, bp =>
        {
            bp.ToTable("schedule_breaks");
            bp.WithOwner().HasForeignKey("schedule_id");
            bp.Property<int>("id").ValueGeneratedOnAdd();
            bp.HasKey("id");
            bp.Property(b => b.BreakStart).HasColumnName("break_start").IsRequired();
            bp.Property(b => b.BreakEnd).HasColumnName("break_end").IsRequired();
        });

        builder.HasIndex(s => new { s.TenantId, s.ProfessionalId })
            .HasDatabaseName("ix_schedules_tenant_professional");

        builder.Ignore(s => s.DomainEvents);
    }
}
