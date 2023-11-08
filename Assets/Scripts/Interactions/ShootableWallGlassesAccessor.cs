﻿namespace Doomlike
{
    using UnityEngine;

    public class ShootableWallGlassesAccessor : MonoBehaviour
    {
        [SerializeField] private ShootableWallGlass[] _wallGlasses = null;
        [SerializeField] private Transform _dbgGlassesParent = null;

        [ContextMenu("Break Glasses")]
        public void BreakGlasses()
        {
            for (int i = _wallGlasses.Length - 1; i >= 0; --i)
                _wallGlasses[i].BreakGlass();
        }

        [ContextMenu("Reset Glasses")]
        public void ResetGlasses()
        {
            for (int i = _wallGlasses.Length - 1; i >= 0; --i)
                _wallGlasses[i].ResetGlass();
        }

        [ContextMenu("Get Wall Glasses in Children")]
        private void GetWallGlassesInChildren()
        {
            _wallGlasses = _dbgGlassesParent != null
                ? _dbgGlassesParent.GetComponentsInChildren<ShootableWallGlass>()
                : GetComponentsInChildren<ShootableWallGlass>();
        }
    }
}