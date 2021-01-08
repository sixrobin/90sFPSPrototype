namespace Doomlike
{
    using UnityEngine;

    public abstract class SwitchTarget : MonoBehaviour
    {
        public bool IsOn { get; protected set; }

        public virtual bool CanToggle { get; set; } = true;

        public virtual void Toggle()
        {
            if (CanToggle)
                IsOn = !IsOn;
        }
    }
}