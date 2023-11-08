namespace Doomlike
{
    using UnityEngine;

    public class TrainingTargetMoving : TrainingTarget
    {
        [SerializeField] private float _amplitude = 0.5f;
        [SerializeField] private float _speed = 2f;
        [SerializeField] private MoveAxis _moveAxis = MoveAxis.X;

        private Vector3 _initPos;
        private Vector3 _movement;
        private float _timer;

        private enum MoveAxis : byte
        {
            X = 1,
            Y = 2,
            Z = 4,
            XY = X | Y,
            XZ = X | Z,
            YZ = Y | Z,
            XYZ = X | Y | Z
        }

        protected override void Update()
        {
            base.Update();

            _timer += Time.deltaTime * _speed;

            if ((_moveAxis & MoveAxis.X) == MoveAxis.X) _movement.x = Mathf.Sin(_timer) * _amplitude;
            if ((_moveAxis & MoveAxis.Y) == MoveAxis.Y) _movement.y = Mathf.Sin(_timer) * _amplitude;
            if ((_moveAxis & MoveAxis.Z) == MoveAxis.Z) _movement.z = Mathf.Sin(_timer) * _amplitude;

            transform.position = _initPos + _movement;
        }

        private void Awake()
        {
            _initPos = transform.position;
        }
    }
}