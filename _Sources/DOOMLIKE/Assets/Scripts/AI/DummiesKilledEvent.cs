namespace Doomlike.AI
{
    using UnityEngine;

    public class DummiesKilledEvent : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private DummyController[] _dummies = null;
        [SerializeField] private UnityEngine.Events.UnityEvent _onDummiesKilled = null;
        [SerializeField] private bool _consoleProMuted = false;

        private int _dummiesLeft;

        public bool ConsoleProMuted => _consoleProMuted;

        public string ConsoleProPrefix => "Dummies Killed Event";

        public delegate void DummiesKilledEventHandler();
        public event DummiesKilledEventHandler DummiesKilled;

        private void OnDummyKilled()
        {
            _dummiesLeft--;
            UnityEngine.Assertions.Assert.IsFalse(_dummiesLeft < 0, "Killed more enemies than the amount registered for the event.");

            if (_dummiesLeft == 0)
            {
                this.Log($"All {_dummies.Length} dummies have been killed, triggering event.", gameObject);
                DummiesKilled?.Invoke();
                _onDummiesKilled?.Invoke();
            }
        }

        private void Start()
        {
            for (int i = _dummies.Length - 1; i >= 0; --i)
            {
                UnityEngine.Assertions.Assert.IsNotNull(_dummies[i], "Array value is null, can not register event!");
                _dummies[i].HealthSystem.Killed += OnDummyKilled;
            }

            _dummiesLeft = _dummies.Length;
        }
    }
}