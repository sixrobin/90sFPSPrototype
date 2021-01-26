namespace Doomlike
{
    using UnityEngine;

    public class EnergyCell : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Collider _collider = null;
        [SerializeField] private MeshRenderer _cellRenderer = null;
        [SerializeField] private ParticleSystem _destroyedParticles = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.3f;

        [SerializeField] private UnityEngine.Events.UnityEvent _onDestroyed = null;

        public delegate void EnergyCellDestroyedEventHandler(EnergyCell cell);
        public event EnergyCellDestroyedEventHandler EnergyCellDestroyed;

        public bool ShotThrough => false;

        public bool IsBulletImpactCrossable => false;

        public float TraumaOnShot => _traumaOnShot;

        public void OnShot(FPSCtrl.FPSShotDatas shotDatas)
        {
            DestroyCell();
        }

        [ContextMenu("Destroy Cell")]
        public void DestroyCell()
        {
            _cellRenderer.enabled = false;
            _collider.enabled = false;

            EnergyCellDestroyed?.Invoke(this);
            _onDestroyed?.Invoke();

            _destroyedParticles.Play();
            Manager.ReferencesHub.FPSMaster.FPSCameraAnimator.PlayHurtAnimation(0.17f);

            if (RSLib.PostProcessController.Exists())
            {
                RSLib.PostProcessController.Instance.BlinkChromaticAberrationWithValues(
                    0.7f,
                    0.1f,
                    0f,
                    0.45f,
                    RSLib.EasingCurves.Curve.InOutSine,
                    RSLib.EasingCurves.Curve.InOutSine);
            }
            else
            {
                Debug.LogWarning("No instance of PostProcessController has been found.");
            }
        }

        [ContextMenu("Restore Cell")]
        public void RestoreCell()
        {
            _cellRenderer.enabled = true;
            _collider.enabled = true;
        }
    }
}