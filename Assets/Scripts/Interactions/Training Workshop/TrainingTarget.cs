namespace Doomlike
{
    using UnityEngine;

    public class TrainingTarget : WorldSpaceBillboard, FPSSystem.IFPSShootable
    {
        private const string ANM_PARAM_SHOT = "Shot";
        private const string ANM_PARAM_RESET = "Reset";

        [SerializeField] private Animator _animator = null;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private GameObject _hitPrefab = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.25f;

        private bool _isDown;

        public delegate void TargetShotEventHandler(TrainingTarget target);

        public event TargetShotEventHandler TargetShot;

        public float TraumaOnShot => _traumaOnShot;

        public bool ShotThrough => false;

        public bool IsBulletImpactCrossable => false;

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            _billboardEnabled = false;
            _animator.enabled = true;
            _animator.SetTrigger(ANM_PARAM_SHOT);

            _collider.enabled = false;

            Transform hitInstanceTransform = Instantiate(_hitPrefab, shotDatas.Point, Quaternion.identity).transform;
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

            _animator.SetTrigger(ANM_PARAM_RESET);
            _billboardEnabled = true;
            _collider.enabled = true; // Requires to make sure player is not overlapping.

            StartCoroutine(ResetTargetCoroutine());
        }

        private System.Collections.IEnumerator ResetTargetCoroutine()
        {
            // Wait for animator transition before disabling it.
            for (int i = 0; i < 3; ++i)
                yield return new WaitForEndOfFrame();

            _animator.enabled = false;
        }
    }
}