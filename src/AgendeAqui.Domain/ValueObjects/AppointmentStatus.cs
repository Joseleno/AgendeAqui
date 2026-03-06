using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ValueObjects;

public sealed class AppointmentStatus : ValueObject
{
    public static readonly AppointmentStatus Scheduled = new("Scheduled");
    public static readonly AppointmentStatus Confirmed = new("Confirmed");
    public static readonly AppointmentStatus InProgress = new("InProgress");
    public static readonly AppointmentStatus Completed = new("Completed");
    public static readonly AppointmentStatus Cancelled = new("Cancelled");
    public static readonly AppointmentStatus NoShow = new("NoShow");

    public static readonly Error InvalidTransition = new("AppointmentStatus.InvalidTransition", "The status transition is not allowed.");

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ValidTransitions =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [Scheduled.Name] = ["Confirmed", "Cancelled"],
            [Confirmed.Name] = ["InProgress", "Cancelled"],
            [InProgress.Name] = ["Completed", "NoShow"],
            [Completed.Name] = [],
            [Cancelled.Name] = [],
            [NoShow.Name] = [],
        };

    public string Name { get; }

    private AppointmentStatus(string name) => Name = name;

    public bool CanTransitionTo(AppointmentStatus target) =>
        ValidTransitions.TryGetValue(Name, out var allowed) && allowed.Contains(target.Name);

    /// <summary>Reconstitutes from a trusted data store (no validation).</summary>
    internal static AppointmentStatus Hydrate(string name) => new(name);

    public static Result<AppointmentStatus> FromName(string name)
    {
        AppointmentStatus? status = name switch
        {
            "Scheduled" => Scheduled,
            "Confirmed" => Confirmed,
            "InProgress" => InProgress,
            "Completed" => Completed,
            "Cancelled" => Cancelled,
            "NoShow" => NoShow,
            _ => null
        };

        return status is null
            ? Result.Failure<AppointmentStatus>(new Error("AppointmentStatus.Unknown", $"Unknown status: {name}"))
            : Result.Success(status);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    public override string ToString() => Name;
}
