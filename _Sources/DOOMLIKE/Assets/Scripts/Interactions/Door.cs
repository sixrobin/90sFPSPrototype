namespace Doomlike
{
    using UnityEngine;

    public class Door : SwitchTarget
    {
        [SerializeField] private Animator _animator = null;
        [SerializeField] private MeshRenderer[] _doorRenderers = null;
        [SerializeField] private Material _triggerShotMat = null;

        public override string ConsoleProPrefix => "Door";

        private bool _openCoroutineRunning;

        private bool _canToggle = true;
        public override bool CanToggle
        {
            get => _canToggle || _openCoroutineRunning;
            protected set
            {
                _canToggle = value;
                if (!_canToggle && !IsOn && !_openCoroutineRunning)
                    this.Log("Locking a door state while it is closed, it then won't open anymore.");
            }
        }

        public void ShootTrigger()
        {
            this.Log("Toggling door by shooting its trigger.", gameObject);

            if (_openCoroutineRunning)
            {
                StopAllCoroutines();
                _openCoroutineRunning = false;
                CanToggle = true;
            }

            Toggle();
            CanToggle = false;

            for (int i = _doorRenderers.Length - 1; i >= 0; --i)
                _doorRenderers[i].material = _triggerShotMat;
        }

        // Unity event on switch destroyed.
        public void OpenAndLock(float delay = 0f)
        {
            this.Log($"Opening <b>{transform.name}</b> with a delay of {delay}s.");

            if (!IsOn && CanToggle)
                StartCoroutine(OpenAndLockCoroutine(delay));

            CanToggle = false;
        }

        public override void Toggle()
        {
            this.Log($"Toggling <b>{transform.name}</b>.");

            if (!CanToggle && !_openCoroutineRunning)
                return;

            base.Toggle();
            this.Log($"Triggering animator parameter \"<b>{(IsOn ? "Open" : "Close")}</b>\".");
            _animator.SetTrigger(IsOn ? "Open" : "Close");
        }

        private System.Collections.IEnumerator OpenAndLockCoroutine(float delay)
        {
            _openCoroutineRunning = true;
            yield return RSLib.Yield.SharedYields.WaitForSeconds(delay);
            Toggle();
            _openCoroutineRunning = false;
        }
    }
}