namespace Doomlike
{
    using UnityEngine;

    public class KeycardLock : OutlinedInteraction, IConsoleProLoggable
    {
        [SerializeField] private int[] _validIds = null;

        public bool ConsoleProMuted => false;

        public string ConsoleProPrefix => "Keycard Lock";

        public override void Focus()
        {
            if (CheckKeycard())
                base.Focus();
        }

        public override void Interact()
        {
            if (CheckKeycard())
                base.Interact();
        }

        private bool CheckKeycard()
        {
            return Manager.ReferencesHub.FPSMaster.FPSInventory.HasAnyKeycard(_validIds);
        }
    }
}