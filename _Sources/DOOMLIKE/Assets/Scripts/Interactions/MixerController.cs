namespace Doomlike
{
    using UnityEngine;

    public class MixerController : MonoBehaviour
    {
        [SerializeField] private ShootableDelegated[] _hitboxes = null;
        [SerializeField] private Shake.ShakeSettings _settings = Shake.ShakeSettings.Default;
        [SerializeField] private GameObject _sparksParticles = null;
        [SerializeField, Range(0f, 1f)] private float _shakeTraumaOnShot = 0.5f;

        private Shake _shake = null;
        private Vector3 _initPos = Vector3.zero;
        private Quaternion _initRot = Quaternion.identity;

        private void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            _shake.SetTrauma(_shakeTraumaOnShot);
            Instantiate(_sparksParticles, shotDatas.Point, _sparksParticles.transform.rotation);
        }

        private void Start()
        {
            for (int i = _hitboxes.Length - 1; i >= 0; --i)
                _hitboxes[i].Shot += OnShot;

            _shake = new Shake(_settings);
            _initPos = transform.position;
            _initRot = transform.rotation;
        }

        private void Update()
        {
            System.Tuple<Vector3, Quaternion> evaluatedShake = _shake.Evaluate(transform);
            if (evaluatedShake == null)
                return;

            transform.position = _initPos + evaluatedShake.Item1;
            transform.rotation = _initRot * evaluatedShake.Item2;
        }

        private void OnDestroy()
        {
            for (int i = _hitboxes.Length - 1; i >= 0; --i)
                _hitboxes[i].Shot -= OnShot;
        }

        [ContextMenu("Get Hitboxes in Children")]
        private void GetHitboxesInChildren()
        {
            _hitboxes = GetComponentsInChildren<ShootableDelegated>();
        }
    }
}