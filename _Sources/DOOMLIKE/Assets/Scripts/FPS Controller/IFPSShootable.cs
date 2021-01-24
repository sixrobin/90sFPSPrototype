namespace Doomlike.FPSCtrl
{
    public interface IFPSShootable
    {
        bool ShotThrough { get; }

        bool IsBulletImpactCrossable { get; }

        float TraumaOnShot { get; }

        void OnShot(FPSShotDatas shotDatas);
    }
}