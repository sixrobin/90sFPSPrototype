using UnityEngine;

public class ShootableWallLight : MonoBehaviour, IFPSShootable
{
    [SerializeField] private Collider _collider = null;
    [SerializeField] private GameObject _light = null;
    [SerializeField] private MeshRenderer _wallMeshRenderer = null;
    [SerializeField] private Material _lightOffWallMat = null;

    public void OnShot(Vector3 point)
    {
        _light.SetActive(false);
        _wallMeshRenderer.material = _lightOffWallMat;
        _collider.enabled = false;
    }
}