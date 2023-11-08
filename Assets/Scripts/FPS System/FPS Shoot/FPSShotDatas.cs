namespace Doomlike.FPSSystem
{
    public class FPSShotDatas
    {
        public UnityEngine.RaycastHit RaycastHit { get; private set; }

        public UnityEngine.Vector3 Point => RaycastHit.point;

        public UnityEngine.Vector3 Normal => RaycastHit.normal;

        public FPSShotDatas(UnityEngine.RaycastHit raycastHit)
        {
            RaycastHit = raycastHit;
        }
    }
}