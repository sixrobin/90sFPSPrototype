namespace Doomlike
{
    using UnityEngine;

    public class HealingCapsuleGlass : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Collider _glassCollider = null;
        [SerializeField] private MeshRenderer _glassRenderer = null;
        [SerializeField] private ParticleSystem _glassShatterParticles = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.25f;

        public bool ShotThrough => true;

        public float TraumaOnShot => _traumaOnShot;

        public void OnShot(Vector3 point)
        {
            _glassCollider.enabled = false;
            _glassRenderer.enabled = false;
            _glassShatterParticles.Play();
        }
    }
}