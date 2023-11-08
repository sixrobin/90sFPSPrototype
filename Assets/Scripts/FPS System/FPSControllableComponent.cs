namespace Doomlike.FPSSystem
{
    /// <summary>
    /// Abstract class that every FPS component that can be enabled or disabled should extend.
    /// Contains a method to control the enable and virtual methods and events called when this variable value is changed;
    /// </summary>
    public abstract class FPSControllableComponent : FPSComponent
    {
        public delegate void ControllableEventHandler();

        public event ControllableEventHandler ControlAllowed;
        public event ControllableEventHandler ControlDisallowed;

        private bool _controllable = true;
        public bool Controllable
        {
            get => _controllable;
            private set
            {
                _controllable = value;

                if (value)
                    OnControlAllowed();
                else
                    OnControlDisallowed();
            }
        }

        /// <summary>
        /// Sets the controllability of the component.
        /// </summary>
        /// <param name="state">Controllability.</param>
        public void SetControllability(bool state)
        {
            Controllable = state;
        }

        /// <summary>
        /// Called when the component controls are allowed. Triggers an event and can be overriden.
        /// </summary>
        protected virtual void OnControlAllowed()
        {
            ControlAllowed?.Invoke();
        }

        /// <summary>
        /// Called when the component controls are disallowed. Triggers an event and can be overriden.
        /// </summary>
        protected virtual void OnControlDisallowed()
        {
            ControlDisallowed?.Invoke();
        }
    }
}