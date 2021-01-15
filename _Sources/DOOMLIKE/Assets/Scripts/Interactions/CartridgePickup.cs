namespace Doomlike
{
    using UnityEngine;

    public class CartridgePickup : OutlinedInteraction
    {
        [Header("-1 FOR FULFILL")]
        [SerializeField] private int _capacity = 100;

        private Cartridge _cartridge;

        public int Capacity => _capacity;

        public override void Interact()
        {
            base.Interact();

            if (Manager.ReferencesHub.FPSMaster.FPSShoot.TryLoadCartridge(_cartridge))
            {
                DisallowInteraction();
                gameObject.SetActive(false);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _cartridge = new Cartridge(_capacity);
        }
    }
}