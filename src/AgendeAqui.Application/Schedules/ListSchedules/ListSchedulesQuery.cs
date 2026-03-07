using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Schedules.GetSchedule;

namespace AgendeAqui.Application.Schedules.ListSchedules;

public sealed record ListSchedulesQuery : PagedRequest, IQuery<PagedResponse<ScheduleResponse>>
{
    public Guid? ProfessionalId { get; init; }

    public ListSchedulesQuery(int page, int pageSize, Guid? professionalId = null)
    {
        Page = page;
        PageSize = pageSize;
        ProfessionalId = professionalId;
    }
}
