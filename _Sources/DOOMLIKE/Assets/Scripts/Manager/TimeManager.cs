namespace Doomlike.Manager
{
    using UnityEngine;

    public class TimeManager : MonoBehaviour, IConsoleProLoggable
    {
        public string ConsoleProPrefix => "Time Manager";

        public void Freeze()
        {
            ConsoleProLogger.Log(this, "Freezing time (setting time scale to 0).", gameObject);
            Time.timeScale = 0f;
        }

        public void Unfreeze()
        {
            ConsoleProLogger.Log(this, "Unfreezing time (setting time scale to 1).", gameObject);
            Time.timeScale = 1f;
        }
    }
}