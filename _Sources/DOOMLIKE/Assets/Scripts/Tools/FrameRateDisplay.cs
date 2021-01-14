namespace Doomlike.Tools
{
    using UnityEngine;

    public class FrameRateDisplay : MonoBehaviour
    {
        [SerializeField] private Color _textColor = new Color(1f, 1f, 0f, 1f);

        private float _deltaTime = 0f;

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            if (!Manager.DebugManager.DbgViewOn)
                return;

            int w = Screen.width;
            int h = Screen.height;

            GUIStyle style = new GUIStyle()
            {
                alignment = TextAnchor.UpperRight,
                padding = new RectOffset(5, 5, 5, 5),
                fontSize = h * 2 / 100,
            };

            style.normal.textColor = _textColor;

            Rect rect = new Rect(0f, 0f, w, h * 2f * 0.01f);
            float ms = _deltaTime * 1000f;
            float fps = 1f / _deltaTime;

            string text = string.Format("{0:0.0} ms ({1:0.} fps)", ms, fps);

            GUI.Label(rect, text, style);
        }
    }
}