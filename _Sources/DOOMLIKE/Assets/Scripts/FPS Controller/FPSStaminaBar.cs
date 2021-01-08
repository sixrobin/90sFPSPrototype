namespace Doomlike.FPSCtrl
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Visual handler of the FPS stamina manager.
    /// </summary>
    public class FPSStaminaBar : MonoBehaviour
    {
        [Header("REFERENCES")]
        [SerializeField] private FPSController _fpsController = null;
        [SerializeField] private GameObject _barHolder = null;
        [SerializeField] private Image _staminaFill = null;

        [Header("STATS")]
        [SerializeField] private float _hideDelay = 2;

        private float _hideTimer;
        private bool _hidden;

        private void Hide()
        {
            _barHolder.SetActive(false);
            _hidden = true;
        }

        private void Show()
        {
            _barHolder.SetActive(true);
            _hidden = false;
        }

        private void UpdateVisual()
        {
            _staminaFill.fillAmount = _fpsController.StaminaManager.CurrentCharge;

            if (_staminaFill.fillAmount == 1)
            {
                if (_hidden)
                    return;

                _hideTimer += Time.deltaTime;
                if (_hideTimer > _hideDelay)
                    Hide();
            }
            else
            {
                _hideTimer = 0;
                if (_hidden)
                    Show();
            }
        }

        private void Start()
        {
            Hide();
        }

        private void Update()
        {
            UpdateVisual();
        }
    }
}