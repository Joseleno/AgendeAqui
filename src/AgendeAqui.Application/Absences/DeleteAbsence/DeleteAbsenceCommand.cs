using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Absences.DeleteAbsence;

public sealed record DeleteAbsenceCommand(Guid AbsenceId) : ICommand;
