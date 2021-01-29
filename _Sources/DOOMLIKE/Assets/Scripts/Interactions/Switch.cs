﻿namespace Doomlike
{
    using UnityEngine;

    public class Switch : OutlinedInteraction, FPSSystem.IFPSShootable
    {
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.1f;
        [SerializeField] private ParticleSystem _smokeParticles = null;
        [SerializeField] private GameObject _sparksParticles = null;

        [SerializeField] private UnityEngine.Events.UnityEvent _onDestroyed = null;

        private bool _destroyed;

        public float TraumaOnShot => _destroyed ? 0f : _traumaOnShot;

        public bool ShotThrough => false;

        public bool IsBulletImpactCrossable => false;

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            if (_destroyed)
                return;

            StartCoroutine(SetAsDestroyedAtEndOfFrame());
            DisallowInteraction();

            _onDestroyed?.Invoke();

            _smokeParticles.transform.position = shotDatas.Point;
            _smokeParticles.Play();
            Instantiate(_sparksParticles, shotDatas.Point, _sparksParticles.transform.rotation);
        }

        private System.Collections.IEnumerator SetAsDestroyedAtEndOfFrame()
        {
            // Return correct trauma on shot before setting the switch as destroyed (and then returning no trauma).
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
            _destroyed = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (_onDestroyed == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);

            for (int i = _onDestroyed.GetPersistentEventCount() - 1; i >= 0; --i)
            {
                if (!(_onDestroyed.GetPersistentTarget(i) is Component listener))
                    continue;

                Gizmos.DrawLine(transform.position, listener.transform.position);
                Gizmos.DrawWireSphere(listener.transform.position, 0.1f);
            }
        }
    }
}