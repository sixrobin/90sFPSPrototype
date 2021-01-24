namespace Doomlike.FPSCtrl
{
    using UnityEngine;

    [System.Serializable]
    public class FPSMagazine : IConsoleProLoggable
    {
        [Header("CAPACITIES")]
        [SerializeField] private int _fullCapacity = 300;
        [SerializeField] private int _loadCapacity = 30;

        [Header("FOR A SPECIFIC CTOR")]
        [SerializeField] private bool _useInitValues = false;
        [SerializeField] private int _initStorehouseCount = 0;
        [SerializeField] private int _initLoadCount = 0;

        public delegate void MagazineValuesChangedEventHandler(FPSMagazine magazine);

        public event MagazineValuesChangedEventHandler MagazineValuesChanged;

        public int FullCapacity
        {
            get => _fullCapacity;
            set => _fullCapacity = Mathf.Max(-1, value); // -1 is an infinite magazine.
        }

        public int LoadCapacity
        {
            get => _loadCapacity;
            set => _loadCapacity = Mathf.Max(0, value);
        }

        public int InitStorehouseCount
        {
            get => _initStorehouseCount;
            set => _initStorehouseCount = Mathf.Max(0, value);
        }

        public int InitLoadCount
        {
            get => _initLoadCount;
            set => _initLoadCount = Mathf.Max(0, value);
        }

        public int CurrLoadCount { get; private set; }

        public int CurrStorehouseCount { get; private set; }

        public bool UseInitValues => _useInitValues;

        public bool CanReload => CurrLoadCount < _loadCapacity && (CurrStorehouseCount > 0 || IsInfinite);

        public bool IsCompletelyEmpty => CurrLoadCount == 0 && CurrStorehouseCount == 0;

        public bool IsFull => CurrLoadCount == LoadCapacity && CurrStorehouseCount == FullCapacity;

        public bool IsLoadEmpty => CurrLoadCount == 0;

        public bool IsInfinite => FullCapacity == -1;

        public string ConsoleProPrefix => "FPS Magazine";

        public bool ConsoleProMuted => Manager.ReferencesHub.Exists() && Manager.ReferencesHub.FPSMaster.FPSShoot.ConsoleProMuted;

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

            if (magazine.UseInitValues)
            {
                CurrStorehouseCount = magazine.InitStorehouseCount;
                CurrLoadCount = magazine.InitLoadCount;

                MagazineValuesChanged?.Invoke(this);
            }
            else
            {
                Fulfill();
            }
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

        public void Empty()
        {
            CurrStorehouseCount = 0;
            CurrLoadCount = 0;

            MagazineValuesChanged?.Invoke(this);
        }

        /// <summary>
        /// Tries to add bullets to the magazine, only if there's space for at least one bullet.
        /// </summary>
        /// <param name="cartridge">Cartridge datas.</param>
        /// <returns>True if space has been found to load, else false.</returns>
        public bool TryLoadCartridge(Cartridge cartridge)
        {
            this.Log($"Loading a cartridge with a capacity of {cartridge.Capacity}.");

            if (IsFull)
            {
                this.Log("Magazine is already full and won't get loaded by cartridge.");
                return false;
            }

            if (cartridge.IsInfinite)
            {
                this.Log("Cartridge is infinite, fulfilling magazine.");
                Fulfill();
                return true;
            }

            if (IsInfinite)
            {
                CurrLoadCount += Mathf.Min(cartridge.Capacity, LoadCapacity - CurrLoadCount);
                return true;
            }

            int leftToUse = cartridge.Capacity;
            int availableStorehouseSpace = FullCapacity - CurrStorehouseCount;

            this.Log($"Adding {Mathf.Min(leftToUse, availableStorehouseSpace)} bullets from cartridge to storehouse.");
            CurrStorehouseCount += Mathf.Min(leftToUse, availableStorehouseSpace);
            leftToUse = Mathf.Max(0, leftToUse - availableStorehouseSpace);

            if (leftToUse > 0)
            {
                int availableLoadSpace = LoadCapacity - CurrLoadCount;

                this.Log($"Adding {Mathf.Min(leftToUse, availableLoadSpace)} bullets from cartridge to load.");
                CurrLoadCount += Mathf.Min(leftToUse, availableLoadSpace);
                leftToUse = Mathf.Max(0, leftToUse - availableLoadSpace);
            }

            this.Log($"{leftToUse} bullet(s) left in cartridge.");

            MagazineValuesChanged?.Invoke(this);
            return true;
        }
    }
}