namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    [System.Serializable]
    public class FPSMagazine
    {
        [SerializeField] private int _fullCapacity = 300;
        [SerializeField] private int _loadCapacity = 30;

        public int FullCapacity
        {
            get => _fullCapacity;
            set => _fullCapacity = value;
        }

        public int LoadCapacity
        {
            get => _loadCapacity;
            set => _loadCapacity = value;
        }

        public int CurrLoadCount { get; private set; }

        public int CurrStorehouseCount { get; private set; }

        public bool CanReload => CurrLoadCount < _loadCapacity && CurrStorehouseCount > 0;

        public bool IsCompletelyEmpty => CurrLoadCount == 0 && CurrStorehouseCount == 0;

        public bool IsLoadEmpty => CurrLoadCount == 0;

        public FPSMagazine(int fullCapacity, int loadCapacity)
        {
            FullCapacity = fullCapacity;
            LoadCapacity = loadCapacity;

            Fulfill();
        }

        public FPSMagazine(FPSMagazine magazine)
        {
            FullCapacity = magazine.FullCapacity;
            LoadCapacity = magazine.LoadCapacity;

            Fulfill();
        }

        public void Shoot()
        {
            if (CurrLoadCount == 0)
                return;

            CurrLoadCount--;
        }

        public void Reload()
        {
            if (!CanReload)
                return;

            int availableLoadSpace = LoadCapacity - CurrLoadCount;
            int reloadCount = UnityEngine.Mathf.Min(availableLoadSpace, CurrStorehouseCount);

            CurrLoadCount += reloadCount;
            CurrStorehouseCount -= reloadCount;
        }

        public void Fulfill()
        {
            CurrStorehouseCount = FullCapacity;
            CurrLoadCount = LoadCapacity;
        }
    }
}