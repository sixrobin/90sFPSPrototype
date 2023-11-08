namespace Doomlike.UI
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public abstract class OptionRaycaster : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private TMPro.TextMeshProUGUI _optionNameText = null;

        private string _optionName;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _optionNameText.text = $" {_optionName}";
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _optionNameText.text = _optionName;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClicked();
        }

        public abstract void Init();

        public abstract void OnClicked();

        protected virtual void Start()
        {
            _optionName = _optionNameText.text;
            Init();
        }
    }
}