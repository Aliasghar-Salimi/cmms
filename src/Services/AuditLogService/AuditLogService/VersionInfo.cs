namespace AuditLogService;

public static class VersionInfo
{
    public static string Version => "1.0.0";
    public static string InformationalVersion => "1.0.0";
    public static string BuildVersion => "1.0.0.0";

    public static object GetVersionInfo()
    {
        return new
        {
            Version = Version,
            InformationalVersion = InformationalVersion,
            BuildVersion = BuildVersion,
            BuildDate = DateTime.UtcNow
        };
    }
} 