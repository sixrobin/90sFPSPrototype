namespace Doomlike
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class CartridgePickup : OutlinedInteraction, FPSSystem.IFPSShootable
    {
        [Header("-1 FOR FULFILL")]
        [SerializeField] private int _capacity = 100;

        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.1f;

        private Cartridge _cartridge;
        private Rigidbody _rb;

        public int Capacity => _capacity;

        public bool ShotThrough => false;

        public float TraumaOnShot => _traumaOnShot;

        public bool IsBulletImpactCrossable => false;

        public override void Interact()
        {
            base.Interact();

            if (Manager.ReferencesHub.FPSMaster.FPSShoot.TryLoadCartridge(_cartridge))
            {
                DisallowInteraction();
                gameObject.SetActive(false);
            }
        }

        public void OnShot(FPSSystem.FPSShotDatas shotDatas)
        {
            RigidbodyBumper.Bump(_rb, 3f, 1f, -0.2f);
        }

        protected override void Awake()
        {
            base.Awake();

            _rb = GetComponent<Rigidbody>();
            _cartridge = new Cartridge(_capacity);
        }

        private void OnGUI()
        {
            if (!Manager.DebugManager.DebugViewOn)
                return;

            Camera mainCamera = Camera.main;
            if ((mainCamera.transform.position - transform.position).sqrMagnitude > 4f)
                return;

            Vector3 worldPos = mainCamera.WorldToScreenPoint(transform.position);
            if (worldPos.z < 0f)
                return;

            worldPos.y = Screen.height - worldPos.y;
            GUI.Label(new Rect(worldPos.x, worldPos.y, 200f, 100f), $"Ammos: {(_capacity == -1 ? "X" : _capacity.ToString())}", Manager.DebugManager.WorldSpaceDebugStyle);
        }
    }
}