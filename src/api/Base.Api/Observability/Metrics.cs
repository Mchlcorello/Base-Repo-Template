using System.Diagnostics.Metrics;

namespace Base.Api.Observability;

public static class Metrics
{
    public const string MeterName = "Base.Api";

    public static readonly Meter Meter = new(MeterName);
}
