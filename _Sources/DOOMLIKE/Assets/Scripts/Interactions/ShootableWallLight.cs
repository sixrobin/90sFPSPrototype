namespace Doomlike
{
    using UnityEngine;

    public class ShootableWallLight : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Collider _collider = null;
        [SerializeField] private GameObject _light = null;
        [SerializeField] private MeshRenderer _wallMeshRenderer = null;
        [SerializeField] private Material _lightOffWallMat = null;
        [SerializeField] private ParticleSystem _breakParticles = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.2f;

        private Material _lightOnWallMat = null;

        public float TraumaOnShot => _traumaOnShot;

        public bool ShotThrough => false;

        public bool IsBulletImpactCrossable => false;

        public void OnShot(FPSCtrl.FPSShotDatas shotDatas)
        {
            _light.SetActive(false);
            _wallMeshRenderer.material = _lightOffWallMat;
            _collider.enabled = false;
            _breakParticles.Play();
        }

        [ContextMenu("Reset Light")]
        public void ResetLight()
        {
            _light.SetActive(true);
            _wallMeshRenderer.material = _lightOnWallMat;
            _collider.enabled = true;
            _breakParticles.gameObject.SetActive(true);
        }

        private void Awake()
        {
            _lightOnWallMat = _wallMeshRenderer.material;
        }
    }
}