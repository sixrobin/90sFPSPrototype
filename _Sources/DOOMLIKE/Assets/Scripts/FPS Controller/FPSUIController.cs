namespace Doomlike.FPSCtrl
{
    using UnityEngine;
    using UnityEngine.UI;

    public class FPSUIController : FPSComponent
    {
        [Header("REFERENCES")]
        [SerializeField] private Canvas _canvas = null;

        [Header("HEALTH")]
        [SerializeField] private Image _healthFill = null;

        [Header("STAMINA")]
        [SerializeField] private GameObject _staminaBarHolder = null;
        [SerializeField] private Image _staminaFill = null;
        [SerializeField] private float _staminaHideDelay = 2f;

        [Header("MAGAZINE")]
        [SerializeField] private TMPro.TextMeshProUGUI _magazineValuesText = null;

        [Header("DEBUG")]
        [SerializeField] private TMPro.TextMeshProUGUI _dbgHealthValueText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _dbgStaminaPercentageText = null;

        private float _staminaHideTimer;
        private bool _staminaHidden;

        private FPSMagazine _fpsMagazine;

        public void Show()
        {
            _canvas.enabled = true;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }

        public void OnMagazineChanged(FPSMagazine magazine)
        {
            if (_fpsMagazine != null && magazine != _fpsMagazine)
                _fpsMagazine.MagazineValuesChanged -= OnMagazineValuesChanged;

            _fpsMagazine = magazine;
            if (_fpsMagazine != null)
                _fpsMagazine.MagazineValuesChanged += OnMagazineValuesChanged;

            OnMagazineValuesChanged(_fpsMagazine);
        }

        private void OnMagazineValuesChanged(FPSMagazine magazine)
        {
            _magazineValuesText.text = $"{magazine.CurrLoadCount} / {(magazine.IsInfinite ? "X" : magazine.CurrStorehouseCount.ToString())}";
        }

        private void ShowStamina()
        {
            _staminaBarHolder.SetActive(true);
            _staminaHidden = false;
        }

        private void HideStamina()
        {
            _staminaBarHolder.SetActive(false);
            _staminaHidden = true;
        }

        private void UpdateHealthView()
        {
            // TODO: Event HealthChanged(previous, new)
            _healthFill.fillAmount = FPSMaster.FPSHealthSystem.HealthSystem.HealthPercentage;
        }

        private void UpdateStaminaView()
        {
            _staminaFill.fillAmount = FPSMaster.FPSController.StaminaManager.CurrentCharge;

            if (_staminaFill.fillAmount == 1f)
            {
                if (_staminaHidden)
                    return;

                _staminaHideTimer += Time.deltaTime;
                if (_staminaHideTimer > _staminaHideDelay)
                    HideStamina();
            }
            else
            {
                _staminaHideTimer = 0f;
                if (_staminaHidden)
                    ShowStamina();
            }
        }

        private void Awake()
        {
            FPSMaster.FPSShoot.MagazineChanged += OnMagazineChanged;
        }

        private void Start()
        {
            HideStamina();
        }

        private void Update()
        {
            UpdateHealthView();
            UpdateStaminaView();

            _dbgHealthValueText.text = Manager.DebugManager.DbgViewOn ? $"{FPSMaster.FPSHealthSystem.HealthSystem.Health} / {FPSMaster.FPSHealthSystem.HealthSystem.MaxHealth}" : string.Empty;
            _dbgStaminaPercentageText.text = Manager.DebugManager.DbgViewOn ? $"{(FPSMaster.FPSController.StaminaManager.CurrentCharge * 100).ToString("f2")}%" : string.Empty;
        }
    }
}