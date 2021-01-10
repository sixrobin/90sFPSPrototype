namespace Doomlike
{
    using UnityEngine;

    public abstract class SwitchTarget : MonoBehaviour, IConsoleProLoggable
    {
        public bool IsOn { get; protected set; }

        public virtual bool CanToggle { get; protected set; } = true;

        public string ConsoleProPrefix => "Switch Target";

        public virtual void Toggle()
        {
            if (CanToggle)
                IsOn = !IsOn;
        }
    }
}