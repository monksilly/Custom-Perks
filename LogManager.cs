using BepInEx.Logging;
namespace CustomPerks;

public static class LogManager
{
    private static ManualLogSource _logger;

    public static void Init(ManualLogSource logger)
    {
        _logger = logger;
    }

    public static void Info(string message)
    {
        _logger?.LogInfo(message);
    }

    public static void Warning(string message)
    {
        _logger?.LogWarning(message);
    }

    public static void Error(string message)
    {
        _logger?.LogError(message);
    }

    public static void Debug(string message)
    {
        _logger?.LogDebug(message);
    }
}

