namespace Doomlike
{
    using RSLib.Extensions;
    using UnityEngine;

    public class VentController : MonoBehaviour
    {
        [SerializeField] private ShootableDelegated[] _hitboxes = null;
        [SerializeField] private float[] _smokesAlphas = null;
        [SerializeField] private ParticleSystem _smokeParticlesPrefab = null;

        private System.Collections.Generic.List<ParticleSystem> _smokes = new System.Collections.Generic.List<ParticleSystem>();

        private void OnShot(FPSCtrl.FPSShotDatas shotDatas)
        {
            ParticleSystem smokeInstance = Instantiate(_smokeParticlesPrefab, transform);
            smokeInstance.transform.position = shotDatas.Point;
            smokeInstance.transform.forward = shotDatas.Normal;

            _smokes.Add(smokeInstance);

            for (int i = _smokes.Count - 1; i >= 0; --i)
            {
                ParticleSystem.MainModule particlesMainModule = _smokes[i].main;
                particlesMainModule.startColor = new ParticleSystem.MinMaxGradient(
                    _smokes[i].main.startColor.color.WithA(_smokesAlphas[Mathf.Min(_smokesAlphas.Length - 1, _smokes.Count - 1)]));
            }
        }

        private void Start()
        {
            for (int i = _hitboxes.Length - 1; i >= 0; --i)
                _hitboxes[i].Shot += OnShot;
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