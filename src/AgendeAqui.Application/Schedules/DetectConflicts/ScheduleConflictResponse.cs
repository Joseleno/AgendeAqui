namespace AgendeAqui.Application.Schedules.DetectConflicts;

public sealed record ScheduleConflictResponse(
    Guid ProfessionalId,
    string ProfessionalName,
    Guid AppointmentId1,
    Guid AppointmentId2,
    string StartTime1,
    string EndTime1,
    string StartTime2,
    string EndTime2,
    string Client1,
    string Client2);
