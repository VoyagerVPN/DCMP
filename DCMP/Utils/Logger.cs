using Serilog;

namespace DCMP.Utils;

public static class Logger
{
    public static void Information(string message) => Log.Information(message);
    public static void Error(string message) => Log.Error(message);
    public static void Warning(string message) => Log.Warning(message);
}
