namespace Doomlike
{
    using RSLib.Extensions;
    using UnityEngine;

    public class WorldSpaceBillboard : MonoBehaviour
    {
        [SerializeField] private Transform _cam = null;
        [SerializeField] private Axis _axis = Axis.None;
        [SerializeField] private bool _reversed = false;

        [SerializeField] private bool _clampRight = false;
        [SerializeField] private bool _clampUp = false;
        [SerializeField] private bool _clampForward = false;

        protected bool _billboardEnabled = true;
        private Vector3 _align;

        protected Transform BillboardCam => _cam;

        public enum Axis
        {
            None,
            Right,
            Up,
            Forward
        }

        protected virtual void Update()
        {
            if (_axis == Axis.None || !_billboardEnabled)
                return;

            _align = _cam.position - transform.position;
            _align.Scale(Vector3.one * (_reversed ? -1f : 1f));

            if (_clampRight) _align = _align.WithX(0f);
            if (_clampUp) _align = _align.WithY(0f);
            if (_clampForward) _align = _align.WithZ(0f);

            switch (_axis)
            {
                case Axis.Right:
                    transform.right = _align;
                    break;

                case Axis.Up: transform.up = _align; break;
                case Axis.Forward: transform.forward = _align; break;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Locate Main Camera")]
        private void LocateMainCamera()
        {
            _cam = Camera.main.transform;
        }
#endif
    }
}