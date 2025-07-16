using System.Reflection;

namespace IdentityService.Application.Common;

public static class VersionInfo
{
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
    public static string FileVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "1.0.0.0";
    public static string InformationalVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
    public static string AssemblyName => Assembly.GetExecutingAssembly().GetName().Name ?? "IdentityService";
    public static string FullName => Assembly.GetExecutingAssembly().GetName().FullName ?? "IdentityService, Version=1.0.0.0";
    
    public static string GetVersionInfo()
    {
        return $"CMMS Identity Service v{InformationalVersion} (Build {Version})";
    }
    
    public static object GetVersionObject()
    {
        return new
        {
            Service = "CMMS Identity Service",
            Version = InformationalVersion,
            BuildVersion = Version,
            FileVersion = FileVersion,
            AssemblyName = AssemblyName,
            BuildDate = GetBuildDate(),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        };
    }
    
    private static DateTime GetBuildDate()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileInfo = new FileInfo(assembly.Location);
            return fileInfo.CreationTime;
        }
        catch
        {
            return DateTime.UtcNow;
        }
    }
} 