namespace Doomlike
{
    using UnityEngine;

    public class Crate : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Rigidbody _rb = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.15f;

        public bool ShotThrough => false;

        public float TraumaOnShot => _traumaOnShot;

        public bool IsBulletImpactCrossable => true;

        public void OnShot(FPSCtrl.FPSShotDatas shotDatas)
        {
            _rb.AddExplosionForce(1000f, shotDatas.Point, 10f, 0f);
        }
    }
}