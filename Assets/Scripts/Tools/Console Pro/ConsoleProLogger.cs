﻿namespace Doomlike
{
    public static class ConsoleProLogger
    {
        public static void Log(IConsoleProLoggable loggable, string log, UnityEngine.GameObject context = null)
        {
            if (!loggable.ConsoleProMuted)
                UnityEngine.Debug.Log($"#{loggable.ConsoleProPrefix}.#{log}", context);
        }

        public static void LogWarning(IConsoleProLoggable loggable, string log, UnityEngine.GameObject context = null)
        {
            if (!loggable.ConsoleProMuted)
                UnityEngine.Debug.LogWarning($"#{loggable.ConsoleProPrefix}.#{log}", context);
        }

        public static void LogError(IConsoleProLoggable loggable, string log, UnityEngine.GameObject context = null)
        {
            if (!loggable.ConsoleProMuted)
                UnityEngine.Debug.LogError($"#{loggable.ConsoleProPrefix}.#{log}", context);
        }

        public static void LogMisc(string log, UnityEngine.GameObject context = null)
        {
            UnityEngine.Debug.Log($"#Misc.#{log}", context);
        }

        public static void LogEditor(string log, UnityEngine.GameObject context = null)
        {
            UnityEngine.Debug.Log($"#Editor.#{log}", context);
        }
    }
}