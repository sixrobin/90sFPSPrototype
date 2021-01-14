namespace Doomlike
{
    using UnityEngine;

    public class Door : SwitchTarget
    {
        [SerializeField] private Animator _animator = null;
        [SerializeField] private MeshRenderer[] _doorRenderers = null;
        [SerializeField] private Material _triggerShotMat = null;

        private bool _canToggle = true;
        public override bool CanToggle
        {
            get => _canToggle;
            protected set
            {
                _canToggle = value;
                if (!_canToggle && !IsOn)
                    Debug.Log("Locking a door state while it is closed, it then won't open anymore.");
            }
        }

        public void ShootTrigger()
        {
            ConsoleProLogger.Log(this, "Toggling door by shooting its trigger.", gameObject);

            Toggle();
            CanToggle = false;

            for (int i = _doorRenderers.Length - 1; i >= 0; --i)
                _doorRenderers[i].material = _triggerShotMat;
        }

        // Unity event on switch destroyed.
        public void OpenAndLock()
        {
            if (!IsOn)
                Toggle();

            CanToggle = false;
        }

        public override void Toggle()
        {
            if (!CanToggle)
                return;

            base.Toggle();
            _animator.SetTrigger(IsOn ? "Open" : "Close");
        }
    }
}