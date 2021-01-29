namespace Doomlike
{
    using UnityEngine;

    public class BulletImpact : MonoBehaviour, FPSSystem.IFPSShootable
    {
        public bool ShotThrough { get; private set; }

        public bool IsBulletImpactCrossable => false;

        public float TraumaOnShot => 0f;

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
        }

        public void SetShotThrough(bool shotThrough)
        {
            ShotThrough = true;
        }

        public void SetTransform(RaycastHit originHit)
        {
            transform.position = originHit.point + originHit.normal * 0.01f;
            transform.forward = -originHit.normal;
            transform.Rotate(0f, 0f, Random.Range(0, 4) * 90);

            Vector3 scale = transform.localScale;
            scale.Scale(new Vector3(1f / originHit.transform.localScale.x, 1f / originHit.transform.localScale.y, 1f / originHit.transform.localScale.z));
            transform.localScale = scale;
        }
    }
}