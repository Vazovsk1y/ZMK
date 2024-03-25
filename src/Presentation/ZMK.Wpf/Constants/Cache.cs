namespace ZMK.Wpf.Constants;

public static class Cache
{
    public const string ProjectsPanelCacheKey = "Projects";
    public static readonly TimeSpan ProjectsPanelCacheExpiration = TimeSpan.FromMinutes(20);

    public const string EmployeesPanelCacheKey = "Employees";
    public static readonly TimeSpan EmployeesPanelCacheExpiration = TimeSpan.FromMinutes(20);
}
