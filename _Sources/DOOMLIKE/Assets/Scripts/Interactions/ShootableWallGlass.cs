﻿namespace Doomlike
{
    using UnityEngine;

    public class ShootableWallGlass : MonoBehaviour, FPSCtrl.IFPSShootable
    {
        [SerializeField] private Collider _collider = null;
        [SerializeField] private MeshRenderer[] _wallMeshRenderers = null;
        [SerializeField] private Material _brokenGlassWallMat = null;
        [SerializeField] private ParticleSystem _glassShatterParticles = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.22f;

        private Material _unbrokenGlassWallMat = null;
        private UnityEngine.AI.NavMeshObstacle _navMeshObstacle = null;

        public float TraumaOnShot => _traumaOnShot;

        public bool ShotThrough => true;

        public void OnShot(Vector3 point)
        {
            for (int i = _wallMeshRenderers.Length - 1; i >= 0; --i)
                _wallMeshRenderers[i].material = _brokenGlassWallMat;
            _collider.enabled = false;

            if (_navMeshObstacle)
                _navMeshObstacle.enabled = false;

            _glassShatterParticles.transform.position = point;
            _glassShatterParticles.Play();
        }

        [ContextMenu("Reset Glass")]
        public void ResetGlass()
        {
            for (int i = _wallMeshRenderers.Length - 1; i >= 0; --i)
                _wallMeshRenderers[i].material = _unbrokenGlassWallMat;
            _collider.enabled = true;

            if (_navMeshObstacle)
                _navMeshObstacle.enabled = true;
        }

        private void Awake()
        {
            _navMeshObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            _unbrokenGlassWallMat = _wallMeshRenderers[0].material;
        }
    }
}