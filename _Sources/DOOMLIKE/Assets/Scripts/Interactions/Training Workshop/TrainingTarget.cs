namespace Doomlike
{
    using UnityEngine;

    public class TrainingTarget : WorldSpaceBillboard, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Animator _animator = null;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private GameObject _hitPrefab = null;
        [SerializeField] private float _traumaOnShot = 0.25f;

        private bool _isDown = false;

        public float TraumaOnShot => _traumaOnShot;

        public delegate void TargetShotEventHandler(TrainingTarget target);
        public event TargetShotEventHandler TargetShot;

        public void OnShot(Vector3 point)
        {
            _billboardEnabled = false;
            _animator.enabled = true;
            _animator.SetTrigger("Shot");

            _collider.enabled = false;

            Transform hitInstanceTransform = Instantiate(_hitPrefab, point, Quaternion.identity).transform;
            hitInstanceTransform.forward = hitInstanceTransform.position - BillboardCam.position;

            _isDown = true;

            TargetShot?.Invoke(this);
        }

        [ContextMenu("Reset Target")]
        public void ResetTarget()
        {
            if (!_isDown)
                return;

            _isDown = false;

            _animator.SetTrigger("Reset");
            _billboardEnabled = true;
            _collider.enabled = true; // Make sure player is not overlapping!

            StartCoroutine(ResetTargetCoroutine());
        }

        private System.Collections.IEnumerator ResetTargetCoroutine()
        {
            for (int i = 0; i < 3; ++i)
                yield return new WaitForEndOfFrame();

            _animator.enabled = false;
        }
    }
}