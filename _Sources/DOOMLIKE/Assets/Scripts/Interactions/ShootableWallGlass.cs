namespace Doomlike
{
    using UnityEngine;

    public class ShootableWallGlass : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Collider _collider = null;
        [SerializeField] private MeshRenderer[] _wallMeshRenderers = null;
        [SerializeField] private Material _brokenGlassWallMat = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.22f;

        private Material _unbrokenGlassWallMat = null;

        public float TraumaOnShot => _traumaOnShot;

        public bool ShotThrough => true;

        public void OnShot(Vector3 point)
        {
            for (int i = _wallMeshRenderers.Length - 1; i >= 0; --i)
                _wallMeshRenderers[i].material = _brokenGlassWallMat;
            _collider.enabled = false;
        }

        [ContextMenu("Reset Glass")]
        public void ResetGlass()
        {
            for (int i = _wallMeshRenderers.Length - 1; i >= 0; --i)
                _wallMeshRenderers[i].material = _unbrokenGlassWallMat;
            _collider.enabled = true;
        }

        private void Awake()
        {
            _unbrokenGlassWallMat = _wallMeshRenderers[0].material;
        }
    }
}