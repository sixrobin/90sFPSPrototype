namespace Doomlike
{
    [System.Serializable]
    public class Cartridge
    {
        [UnityEngine.SerializeField] private int _capacity;

        public int Capacity
        {
            get => _capacity;
            set => _capacity = value;
        }

        public bool IsInfinite => Capacity == -1;

        public Cartridge(int capacity)
        {
            Capacity = capacity;
        }
    }
}