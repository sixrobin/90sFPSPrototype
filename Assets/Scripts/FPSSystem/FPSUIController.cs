namespace Doomlike.FPSSystem
{
    using UnityEngine;
    using UnityEngine.UI;

    public class FPSUIController : FPSComponent
    {
        private const string ANM_PARAM_HEALTH_CHANGED = "HealthChanged";
        private const string ANM_PARAM_CANT_SHOOT = "CantShoot";
        private const string ANM_PARAM_ALREADY_FULL = "AlreadyFull";
        private const string INFINITE_MAGAZINE_TEXT = "X";

        [Header("REFERENCES")]
        [SerializeField] private Canvas _canvas = null;

        [Header("HEALTH")]
        [SerializeField] private Image _healthFill = null;
        [SerializeField] private Image _healthBlink = null;
        [SerializeField] private Animator _healthBarAnimator = null;

        [Header("STAMINA")]
        [SerializeField] private GameObject _staminaBarHolder = null;
        [SerializeField] private Image _staminaFill = null;
        [SerializeField] private float _staminaHideDelay = 2f;

        [Header("MAGAZINE")]
        [SerializeField] private TMPro.TextMeshProUGUI _magazineValuesText = null;
        [SerializeField] private Animator _magazineTextAnimator = null;

        [Header("DEBUG")]
        [SerializeField] private TMPro.TextMeshProUGUI _debugHealthValueText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _debugStaminaPercentageText = null;

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
            _magazineValuesText.text = $"{magazine.CurrLoadCount} / {(magazine.IsInfinite ? INFINITE_MAGAZINE_TEXT : magazine.CurrStorehouseCount.ToString())}";
        }

        private void OnTriedShot(FPSShoot.ShootInputResult result)
        {
            if (result == FPSShoot.ShootInputResult.Failure)
                _magazineTextAnimator.SetTrigger(ANM_PARAM_CANT_SHOOT);
        }

        private void OnCartridgeLoaded(bool result)
        {
            if (!result)
                _magazineTextAnimator.SetTrigger(ANM_PARAM_ALREADY_FULL);
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

        private void OnHealthChanged(int newHealth)
        {
            _healthFill.fillAmount = FPSMaster.FPSHealthSystem.HealthSystem.HealthPercentage;
            _healthBlink.fillAmount = _healthFill.fillAmount;
            _healthBarAnimator.SetTrigger(ANM_PARAM_HEALTH_CHANGED);
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
            FPSMaster.FPSShoot.TriedShot += OnTriedShot;
            FPSMaster.FPSShoot.CartridgeLoaded += OnCartridgeLoaded;
            FPSMaster.FPSHealthSystem.HealthSystem.HealthChanged += OnHealthChanged;

            HideStamina();
        }

        private void Update()
        {
            UpdateStaminaView();

            _debugHealthValueText.text = Manager.DebugManager.DebugViewOn ? $"{FPSMaster.FPSHealthSystem.HealthSystem.Health} / {FPSMaster.FPSHealthSystem.HealthSystem.MaxHealth}" : string.Empty;
            _debugStaminaPercentageText.text = Manager.DebugManager.DebugViewOn ? $"{(this.FPSMaster.FPSController.StaminaManager.CurrentCharge * 100):f2}%" : string.Empty;
        }

        private void OnDestroy()
        {
            FPSMaster.FPSShoot.MagazineChanged -= OnMagazineChanged;
            FPSMaster.FPSShoot.TriedShot -= OnTriedShot;
            FPSMaster.FPSShoot.CartridgeLoaded -= OnCartridgeLoaded;
            FPSMaster.FPSHealthSystem.HealthSystem.HealthChanged -= OnHealthChanged;
        }
    }
}