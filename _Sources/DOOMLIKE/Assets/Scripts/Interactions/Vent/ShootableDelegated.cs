namespace Doomlike
{
    using UnityEngine;

    public class ShootableDelegated : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.25f;
        [SerializeField] private UnityEngine.Events.UnityEvent _onShot = null;

        public delegate void ShotEventHandler(FPSCtrl.FPSShotDatas shotDatas);
        public event ShotEventHandler Shot;

        public bool ShotThrough => false;

        public float TraumaOnShot => _traumaOnShot;

        public void OnShot(FPSCtrl.FPSShotDatas shotDatas)
        {
            Shot?.Invoke(shotDatas);
            _onShot.Invoke();
        }
    }
}