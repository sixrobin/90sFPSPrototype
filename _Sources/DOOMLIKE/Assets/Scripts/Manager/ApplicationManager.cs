namespace Doomlike.Manager
{
    public class ApplicationManager
    {
        public static void LogRealtimeSinceStartup()
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(UnityEngine.Time.realtimeSinceStartup);
            string timeFormat = string.Format(
                "{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);

            ConsoleProLogger.LogMisc($"Realtime since startup: {timeFormat}.");
        }

        public static void OpenPersistentDataPath()
        {
            string path = UnityEngine.Application.persistentDataPath;
            ConsoleProLogger.LogMisc($"Opening persistent data path: {path}");
            System.Diagnostics.Process.Start(path);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}