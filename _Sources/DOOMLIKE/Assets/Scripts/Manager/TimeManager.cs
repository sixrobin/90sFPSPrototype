﻿namespace Doomlike.Manager
{
    using UnityEngine;

    public class TimeManager : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private bool _logsMuted = false;

        public string ConsoleProPrefix => "Time Manager";

        public bool ConsoleProMuted => _logsMuted;

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

        private void OnDebugConsoleToggled(bool state)
        {
            if (state)
                Freeze();
            else
                Unfreeze();
        }

        private void Awake()
        {
            Console.DebugConsole.Instance.DebugConsoleToggled += OnDebugConsoleToggled;
        }

        private void OnDestroy()
        {
            if (Console.DebugConsole.Exists())
                Console.DebugConsole.Instance.DebugConsoleToggled -= OnDebugConsoleToggled;
        }
    }
}