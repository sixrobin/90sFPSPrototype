namespace Doomlike
{
    using RSLib.Extensions;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class CartridgePickup : OutlinedInteraction, FPSCtrl.IFPSShootable
    {
        [Header("-1 FOR FULFILL")]
        [SerializeField] private int _capacity = 100;

        [SerializeField, Range(0f, 1f)] private float _traumaOnShot = 0.1f;

        private Cartridge _cartridge;
        private Rigidbody _rb;

        public int Capacity => _capacity;

        public bool ShotThrough => false;

        public float TraumaOnShot => _traumaOnShot;

        public override void Interact()
        {
            base.Interact();

            if (Manager.ReferencesHub.FPSMaster.FPSShoot.TryLoadCartridge(_cartridge))
            {
                DisallowInteraction();
                gameObject.SetActive(false);
            }
        }

        public void OnShot(Vector3 point)
        {
            float rndX = Random.Range(0.5f, 1f);
            float rndZ = Random.Range(0.5f, 1f);

            if (transform.up.y < 0.5f)
                transform.up = transform.up.WithY(1f);

            for (int i = 0; i < 3; ++i)
            {
                _rb.AddExplosionForce(3f, point.AddY(-0.2f), 1f, 3f);
                _rb.velocity = transform.TransformDirection(new Vector3(rndX, 10f, rndZ));
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _rb = GetComponent<Rigidbody>();
            _cartridge = new Cartridge(_capacity);
        }

        private void OnGUI()
        {
            if (!Manager.DebugManager.DbgViewOn)
                return;

            Camera mainCamera = Camera.main;

            if ((mainCamera.transform.position - transform.position).sqrMagnitude > 4f)
                return;

            Vector3 worldPos = mainCamera.WorldToScreenPoint(transform.position);
            if (worldPos.z < 0f)
                return;

            GUIStyle dbgStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 1f, 1f, 1f)
                }
            };

            worldPos.y = Screen.height - worldPos.y;
            GUI.Label(new Rect(worldPos.x, worldPos.y, 200f, 100f), $"Ammos: {(_capacity == -1 ? "X" : _capacity.ToString())}", dbgStyle);
        }
    }
}