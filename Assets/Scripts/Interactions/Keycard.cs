namespace Doomlike
{
    using UnityEngine;

    public class Keycard : OutlinedInteraction, IConsoleProLoggable
    {
        [SerializeField] private int _id = 0;

        public int Id => _id;

        public bool ConsoleProMuted => false;

        public string ConsoleProPrefix => "Keycard";

        public override void Interact()
        {
            base.Interact();

            Manager.ReferencesHub.FPSMaster.FPSInventory.AddKeycard(_id);
            gameObject.SetActive(false);

            this.Log($"Picked up Keycard <b>n°{Id}</b>.");
        }
    }
}