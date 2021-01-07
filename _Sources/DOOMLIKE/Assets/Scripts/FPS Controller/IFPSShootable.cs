public interface IFPSShootable 
{
    float TraumaOnShot { get; }

    void OnShot(UnityEngine.Vector3 point);
}