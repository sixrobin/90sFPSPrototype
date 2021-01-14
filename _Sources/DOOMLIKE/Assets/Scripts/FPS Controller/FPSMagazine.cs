namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    [System.Serializable]
    public class FPSMagazine
    {
        [SerializeField] private int _fullCapacity = 300;
        [SerializeField] private int _loadCapacity = 30;

        public delegate void MagazineValuesChangedEventHandler(FPSMagazine magazine);

        public event MagazineValuesChangedEventHandler MagazineValuesChanged;

        // If set to -1, means an infinite magazine.
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

        public bool CanReload => CurrLoadCount < _loadCapacity && (CurrStorehouseCount > 0 || IsInfinite);

        public bool IsCompletelyEmpty => CurrLoadCount == 0 && CurrStorehouseCount == 0;

        public bool IsLoadEmpty => CurrLoadCount == 0;

        public bool IsInfinite => FullCapacity == -1;

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
            MagazineValuesChanged?.Invoke(this);
        }

        public void Reload()
        {
            if (!CanReload)
                return;

            int availableLoadSpace = LoadCapacity - CurrLoadCount;
            int reloadCount = IsInfinite ? availableLoadSpace : Mathf.Min(availableLoadSpace, CurrStorehouseCount);

            CurrLoadCount += reloadCount;
            if (!IsInfinite)
                CurrStorehouseCount -= reloadCount;

            MagazineValuesChanged?.Invoke(this);
        }

        public void Fulfill()
        {
            CurrStorehouseCount = FullCapacity;
            CurrLoadCount = LoadCapacity;

            MagazineValuesChanged?.Invoke(this);
        }
    }
}