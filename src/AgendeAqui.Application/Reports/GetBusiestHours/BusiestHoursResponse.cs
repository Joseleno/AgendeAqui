namespace AgendeAqui.Application.Reports.GetBusiestHours;

public sealed record BusiestHoursResponse(List<HourSlot> Slots);

public sealed record HourSlot(int DayOfWeek, int Hour, int Count);
