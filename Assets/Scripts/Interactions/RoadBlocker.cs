﻿namespace Doomlike
{
    using UnityEngine;

    public class RoadBlocker : MonoBehaviour, FPSSystem.IFPSShootable
    {
        private const string ANM_PARAM_SHOT = "Shot";

        [SerializeField] private Animator _animator = null;
        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.15f;

        public bool ShotThrough => false;

        public float TraumaOnShot => _traumaOnShot;

        public bool IsBulletImpactCrossable => true;

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            _animator.SetTrigger(ANM_PARAM_SHOT);
        }
    }
}