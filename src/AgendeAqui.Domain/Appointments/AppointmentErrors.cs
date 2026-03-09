using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Appointments;

public static class AppointmentErrors
{
    public static readonly Error NotFound = new("Appointment.NotFound", "Appointment not found.");
    public static readonly Error InvalidTransition = new("Appointment.InvalidTransition", "The requested status transition is not allowed.");
    public static readonly Error Conflict = new("Appointment.Conflict", "There is already an appointment scheduled for this time slot.");
    public static readonly Error ProfessionalNotFound = new("Appointment.ProfessionalNotFound", "The specified professional was not found.");
    public static readonly Error ServiceNotFound = new("Appointment.ServiceNotFound", "The specified service was not found.");
    public static readonly Error ClientNotFound = new("Appointment.ClientNotFound", "The specified client was not found.");
    public static readonly Error ProfessionalInactive = new("Appointment.ProfessionalInactive", "The specified professional is not active.");
    public static readonly Error ServiceInactive = new("Appointment.ServiceInactive", "The specified service is not active.");
    public static readonly Error InvalidDate = new("Appointment.InvalidDate", "Appointment date must be in the future.");
    public static readonly Error NoSchedule = new("Appointment.NoSchedule", "The professional has no schedule configured for this day.");
    public static readonly Error OutsideSchedule = new("Appointment.OutsideSchedule", "The requested time is outside the professional's working hours.");
    public static readonly Error CancelReasonRequired = new("Appointment.CancelReasonRequired", "A reason is required to cancel an appointment.");
    public static readonly Error InvalidTenant = new("Appointment.InvalidTenant", "A valid tenant identifier is required.");
}
