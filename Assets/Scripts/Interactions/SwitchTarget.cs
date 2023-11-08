namespace Doomlike
{
    using UnityEngine;

    public abstract class SwitchTarget : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private bool _logsMuted = false;

        public bool IsOn { get; protected set; }

        public virtual bool CanToggle { get; protected set; } = true;

        public virtual string ConsoleProPrefix => "Switch Target";

        public virtual bool ConsoleProMuted => _logsMuted;

        public virtual void Toggle()
        {
            if (CanToggle)
                IsOn = !IsOn;
        }
    }
}