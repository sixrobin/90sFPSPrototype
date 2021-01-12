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

        private float _staminaHideTimer;
        private bool _staminaHidden;

        public void Show()
        {
            _canvas.enabled = true;
        }

        public void Hide()
        {
            _canvas.enabled = false;
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
                    Hide();
            }
            else
            {
                _staminaHideTimer = 0f;
                if (_staminaHidden)
                    Show();
            }
        }


        private void Start()
        {
            HideStamina();
        }

        private void Update()
        {
            UpdateHealthView();
            UpdateStaminaView();
        }
    }
}