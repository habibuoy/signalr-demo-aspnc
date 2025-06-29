using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SignalRDemo.Server.Datas;

public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter()
        : base(v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {

    }
}

public class NullableDateTimeUtcConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableDateTimeUtcConverter()
        : base(v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {

    }
}

// use this attribute for DateTime properties that are not in UTC
[AttributeUsage(AttributeTargets.Property)]
public class NotUtcAttribute : Attribute { }