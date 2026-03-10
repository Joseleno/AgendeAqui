using System.Data;
using Dapper;

namespace AgendeAqui.Infrastructure.Persistence;

internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => value switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateOnly")
    };

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}

internal sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value) => value switch
    {
        TimeOnly t => t,
        TimeSpan ts => TimeOnly.FromTimeSpan(ts),
        DateTime dt => TimeOnly.FromDateTime(dt),
        _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to TimeOnly")
    };

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}

internal sealed class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value) => value switch
    {
        DateTime dt => dt,
        DateTimeOffset dto => dto.UtcDateTime,
        _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateTime")
    };

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.DbType = DbType.DateTime;
        parameter.Value = value;
    }
}

internal static class DapperTypeHandlers
{
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeHandler());
    }
}
