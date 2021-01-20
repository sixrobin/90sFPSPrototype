namespace Doomlike
{
    public static class ConsoleProLoggableExtensions
    {
        public static void Log(this IConsoleProLoggable loggable, string log, UnityEngine.GameObject context = null)
        {
            ConsoleProLogger.Log(loggable, log, context);
        }

        public static void LogWarning(this IConsoleProLoggable loggable, string log, UnityEngine.GameObject context = null)
        {
            ConsoleProLogger.LogWarning(loggable, log, context);
        }

        public static void LogError(this IConsoleProLoggable loggable, string log, UnityEngine.GameObject context = null)
        {
            ConsoleProLogger.LogError(loggable, log, context);
        }
    }
}