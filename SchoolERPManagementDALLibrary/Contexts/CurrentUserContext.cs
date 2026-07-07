using System.Threading;

namespace SchoolERPManagementDALLibrary.Contexts;

public static class CurrentUserContext
{
    private static readonly AsyncLocal<string?> _currentUsername = new();

    public static string? Username
    {
        get => _currentUsername.Value;
        set => _currentUsername.Value = value;
    }
}
