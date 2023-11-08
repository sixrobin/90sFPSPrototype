namespace Doomlike.FPSSystem
{
    using System.Linq;

    public class FPSInventory : IConsoleProLoggable
    {
        private System.Collections.Generic.List<int> _keycards;

        public FPSInventory()
        {
            _keycards = new System.Collections.Generic.List<int>();
        }

        public bool ConsoleProMuted => false;

        public string ConsoleProPrefix => "FPS Inventory";

        public void AddKeycard(int id)
        {
            if (_keycards.Contains(id))
                this.Log($"Duplicate Id for keycard <b>{id}</b>");

            _keycards.Add(id);
        }

        public bool HasKeycard(int id)
        {
            return _keycards.Contains(id);
        }

        public bool HasAnyKeycard(System.Collections.Generic.IEnumerable<int> ids)
        {
            return _keycards.Intersect(ids).Count() > 0;
        }
    }
}