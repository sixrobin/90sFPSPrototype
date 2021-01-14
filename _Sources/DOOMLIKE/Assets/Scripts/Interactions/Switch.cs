namespace Doomlike
{
    using UnityEngine;

    public class Switch : OutlinedInteraction, FPSCtrl.IFPSShootable
    {
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.1f;
        [SerializeField] private ParticleSystem _smokeParticles = null;
        [SerializeField] private GameObject _sparksParticles = null;

        [SerializeField] private UnityEngine.Events.UnityEvent _onDestroyed = null;

        private bool _destroyed;

        public float TraumaOnShot => _destroyed ? 0f : _traumaOnShot;

        public void OnShot(Vector3 point)
        {
            if (_destroyed)
                return;

            StartCoroutine(SetAsDestroyedAtEndOfFrame());
            DisallowInteraction();

            _onDestroyed?.Invoke();

            _smokeParticles.transform.position = point;
            _smokeParticles.Play();
            Instantiate(_sparksParticles, point, _sparksParticles.transform.rotation);
        }

        private System.Collections.IEnumerator SetAsDestroyedAtEndOfFrame()
        {
            // Return correct trauma on shot before setting the switch as destroyed (and then returning no trauma).
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
            _destroyed = true;
        }
    }
}