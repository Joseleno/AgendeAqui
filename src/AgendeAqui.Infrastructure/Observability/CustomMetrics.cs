using System.Diagnostics.Metrics;

namespace AgendeAqui.Infrastructure.Observability;

public sealed class CustomMetrics
{
    public const string MeterName = "AgendeAqui";

    private readonly Counter<long> _appointmentsCreated;
    private readonly Counter<long> _appointmentsByStatus;
    private readonly Histogram<double> _availabilityQueryDuration;
    private readonly Counter<long> _messagesProcessed;
    private readonly Counter<long> _messagesFailed;
    private readonly Histogram<double> _messageProcessingDuration;

    public CustomMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _appointmentsCreated = meter.CreateCounter<long>(
            "appointments.created.total",
            description: "Total appointments created");

        _appointmentsByStatus = meter.CreateCounter<long>(
            "appointments.by_status",
            description: "Appointments by status");

        _availabilityQueryDuration = meter.CreateHistogram<double>(
            "availability.query.duration.seconds",
            unit: "s",
            description: "Availability query duration");

        _messagesProcessed = meter.CreateCounter<long>(
            "messages.processed.total",
            description: "Total messages processed");

        _messagesFailed = meter.CreateCounter<long>(
            "messages.failed.total",
            description: "Total messages failed");

        _messageProcessingDuration = meter.CreateHistogram<double>(
            "messages.processing.duration.seconds",
            unit: "s",
            description: "Message processing duration");
    }

    public void RecordAppointmentCreated(string tenantId) =>
        _appointmentsCreated.Add(1, new KeyValuePair<string, object?>("tenant_id", tenantId));

    public void RecordAppointmentStatus(string status) =>
        _appointmentsByStatus.Add(1, new KeyValuePair<string, object?>("status", status));

    public void RecordAvailabilityQueryDuration(double seconds) =>
        _availabilityQueryDuration.Record(seconds);

    public void RecordMessageProcessed(string queue) =>
        _messagesProcessed.Add(1, new KeyValuePair<string, object?>("queue", queue));

    public void RecordMessageFailed(string queue) =>
        _messagesFailed.Add(1, new KeyValuePair<string, object?>("queue", queue));

    public void RecordMessageProcessingDuration(string queue, double seconds) =>
        _messageProcessingDuration.Record(seconds, new KeyValuePair<string, object?>("queue", queue));
}
