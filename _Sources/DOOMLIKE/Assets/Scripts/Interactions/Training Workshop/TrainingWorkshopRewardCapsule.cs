namespace Doomlike
{
    using UnityEngine;

    public class TrainingWorkshopRewardCapsule : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [Header("REFERENCES")]
        [SerializeField] private TrainingWorkshop _trainingWorkshop = null;
        [SerializeField] private GameObject _reward = null;
        [SerializeField] private Transform[] _capsuleBody = null;

        [Header("CAPSULE ROTATION")]
        [SerializeField, Min(0f)] private float _capsuleBodyBaseRotSpeed = 35f;
        [SerializeField, Min(0f)] private float _shotRotSpeed = 270f;
        [SerializeField, Min(0f)] private float _rotBrakeDur = 1f;
        [SerializeField] private RSLib.EasingCurves.Curve _rotBrakeCurve = RSLib.EasingCurves.Curve.OutQuad;

        [Space]
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.1f;

        private bool _rewardObtained = false;
        private float _currRotSpeed = 0f;
        private bool _rotSpeedBraking = false;

        public bool ShotThrough => false;

        public float TraumaOnShot => _traumaOnShot;

        public void OnShot(FPSCtrl.FPSShotDatas shotDatas)
        {
            _currRotSpeed = _shotRotSpeed;
            StopAllCoroutines();
            StartCoroutine(BrakeRotationCoroutine());
        }

        private void RotateCapsuleBody()
        {
            for (int i = _capsuleBody.Length - 1, dir = 1; i >= 0; --i, dir *= -1)
                _capsuleBody[i].Rotate(0f, _currRotSpeed * dir * Time.deltaTime, 0f, Space.World);
        }

        private System.Collections.IEnumerator BrakeRotationCoroutine()
        {
            _rotSpeedBraking = true;

            for (float t = 0; t < 1f; t += Time.deltaTime / _rotBrakeDur)
            {
                _currRotSpeed = RSLib.Maths.Maths.NormalizeClamped(
                    RSLib.EasingCurves.Easing.Ease(t, _rotBrakeCurve),
                    1f,
                    0f,
                    _capsuleBodyBaseRotSpeed,
                    _shotRotSpeed);

                yield return null;
            }

            _currRotSpeed = _capsuleBodyBaseRotSpeed;
            _rotSpeedBraking = false;
        }

        private void Awake()
        {
            _trainingWorkshop.WorkshopComplete += OnWorkshopComplete;
        }

        private void OnWorkshopComplete(int score)
        {
            if (_rewardObtained || score > 0)
                return;

            _rewardObtained = true;
            _reward.SetActive(true);
        }

        private void Update()
        {
            if (!_rotSpeedBraking)
                _currRotSpeed = _capsuleBodyBaseRotSpeed;

            RotateCapsuleBody();
        }

        [ContextMenu("Obtain Reward")]
        private void DBG_ObtainReward()
        {
            _reward.SetActive(true);
        }
    }
}