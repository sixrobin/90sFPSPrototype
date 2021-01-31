namespace Doomlike
{
    using UnityEngine;

    public class Door : SwitchTarget
    {
        [SerializeField] private Animator _animator = null;
        [SerializeField] private MeshRenderer[] _doorRenderers = null;
        [SerializeField] private Material _triggerShotMat = null;
        [SerializeField] private bool _openOnStart = false;

        public override string ConsoleProPrefix => "Door";

        private bool _toggleCoroutineRunning;

        private bool _canToggle = true;
        public override bool CanToggle
        {
            get => _canToggle || _toggleCoroutineRunning;
            protected set
            {
                _canToggle = value;
                if (!_canToggle && !_toggleCoroutineRunning)
                {
                    if (!IsOn)
                        this.Log("Locking a door state while it is closed, it then won't open anymore.", gameObject);
                    else
                        this.Log("Locking a door state while it is open, it then won't close anymore.", gameObject);
                }
            }
        }

        public void ShootTrigger()
        {
            this.Log("Toggling door by shooting its trigger.", gameObject);

            if (_toggleCoroutineRunning)
            {
                StopAllCoroutines();
                _toggleCoroutineRunning = false;
                CanToggle = true;
            }

            Toggle();
            CanToggle = false;

            for (int i = _doorRenderers.Length - 1; i >= 0; --i)
                _doorRenderers[i].material = _triggerShotMat;
        }

        public void OpenAndLock(float delay = 0f)
        {
            this.Log($"Opening <b>{transform.name}</b> with a delay of {delay}s.", gameObject);

            if (!IsOn && CanToggle)
                StartCoroutine(ToggleDelayedCoroutine(delay));

            CanToggle = false;
        }

        public void CloseAndLock(float delay = 0f)
        {
            this.Log($"Closing <b>{transform.name}</b> with a delay of {delay}s.", gameObject);

            if (IsOn && CanToggle)
                StartCoroutine(ToggleDelayedCoroutine(delay));

            CanToggle = false;
        }

        public override void Toggle()
        {
            this.Log($"Toggling <b>{transform.name}</b>.", gameObject);

            if (!CanToggle && !_toggleCoroutineRunning)
                return;

            base.Toggle();
            this.Log($"Triggering animator parameter \"<b>{(IsOn ? "Open" : "Close")}</b>\".", gameObject);
            _animator.SetTrigger(IsOn ? "Open" : "Close");
        }

        public void ToggleDelayed(float delay)
        {
            if (_toggleCoroutineRunning)
                return;

            if (delay == 0f)
                Toggle();
            else
                StartCoroutine(ToggleDelayedCoroutine(delay));
        }

        private System.Collections.IEnumerator ToggleDelayedCoroutine(float delay)
        {
            _toggleCoroutineRunning = true;
            yield return RSLib.Yield.SharedYields.WaitForSeconds(delay);
            Toggle();
            _toggleCoroutineRunning = false;
        }

        private void Start()
        {
            if (_openOnStart)
                Toggle();
        }
    }
}