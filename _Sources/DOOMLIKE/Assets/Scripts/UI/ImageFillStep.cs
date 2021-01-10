namespace Doomlike.UI
{
    using UnityEngine;

    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class ImageFillStep : MonoBehaviour
    {
        [SerializeField] private float _step = 0.03f;

        private UnityEngine.UI.Image _image;

        private void Start()
        {
            _image = GetComponent<UnityEngine.UI.Image>();
        }

        private void Update()
        {
            if (_step == 0f)
                return;

            float target = 0f;
            for (float f = 0f; f <= 1f; f += _step)
            {
                if (f < _image.fillAmount)
                {
                    target = f;
                    continue;
                }

                break;
            }

            _image.fillAmount = target;
        }
    }
}