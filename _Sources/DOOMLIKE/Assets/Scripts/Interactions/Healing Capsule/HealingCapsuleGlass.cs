namespace Doomlike
{
    using UnityEngine;

    public class HealingCapsuleGlass : MonoBehaviour, FPSSystem.IFPSShootable
    {
        [SerializeField] private Collider _glassCollider = null;
        [SerializeField] private MeshRenderer _glassRenderer = null;
        [SerializeField] private ParticleSystem _glassShatterParticles = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.25f;

        public bool ShotThrough => true;

        public float TraumaOnShot => _traumaOnShot;

        public bool IsBulletImpactCrossable => false;

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            _glassCollider.enabled = false;
            _glassRenderer.enabled = false;
            _glassShatterParticles.Play();
        }
    }
}