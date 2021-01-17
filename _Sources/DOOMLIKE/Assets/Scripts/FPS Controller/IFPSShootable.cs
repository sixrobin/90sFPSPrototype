namespace Doomlike.FPSCtrl
{
    public interface IFPSShootable
    {
        bool ShotThrough { get; }

        float TraumaOnShot { get; }

        void OnShot(UnityEngine.Vector3 point);
    }
}