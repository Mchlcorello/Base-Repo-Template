using System.Diagnostics;

namespace Base.Api.Observability;

public static class ActivitySources
{
    public const string SourceName = "Base.Api";

    public static readonly ActivitySource Source = new(SourceName);
}
