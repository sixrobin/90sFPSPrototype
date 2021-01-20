namespace Doomlike.UI
{
    using RSLib.Extensions;
    using UnityEngine;

    public class TrainingWorkshopTerminalScreen : MonoBehaviour, IConsoleProLoggable
    {
        [SerializeField] private Canvas _canvas = null;
        [SerializeField] private GameObject _screenShatteredView = null;
        [SerializeField] private TrainingWorkshopTerminal[] _terminals = null;
        [SerializeField] private TMPro.TextMeshProUGUI _titleText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _triesText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _bestShotsText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _bestTimeText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _scoreText = null;
        [SerializeField] private Color _scoreHighlightColor = Color.white;
        [SerializeField] private float _bestTimeSecondsTextSize = 2.5f;

        [Header("DEBUG")]
        [SerializeField] private bool _logsMuted = false;

        private string[] _scoresString = new string[] { "S", "A", "B", "C", "D" }; // 01234 = SABCD.
        private Color _scoreBaseColor;

        public delegate void TerminalScreenToggledEventHandler(bool state);

        public event TerminalScreenToggledEventHandler TerminalScreenToggled;

        public string ConsoleProPrefix => "Training Workshop";

        public bool ConsoleProMuted => _logsMuted;

        private void OnTerminalInteracted(FPSCtrl.FPSInteraction interaction)
        {
            this.Log($"Turning on Training Workshop terminal screen.");

            // TODO: Dictionary<Interaction, Terminal> to avoid cast?
            TrainingWorkshopTerminal terminal = interaction as TrainingWorkshopTerminal;
            UnityEngine.Assertions.Assert.IsTrue(terminal != null);

            _canvas.enabled = true;
            _screenShatteredView.SetActive(terminal.ScreenShattered);

            _titleText.text = $"Training\nWorkshop #<size=1.5> </size>{terminal.TrainingWorkshop.WorkshopIndex}";
            _triesText.text = terminal.TrainingWorkshop.Tries.ToString();
            _bestShotsText.text = terminal.TrainingWorkshop.BestShots == int.MaxValue ? "0" : terminal.TrainingWorkshop.BestShots.ToString();
            _bestTimeText.text = terminal.TrainingWorkshop.BestTime == float.MaxValue ? "0.0" : $"{terminal.TrainingWorkshop.BestTime:f2}<size={_bestTimeSecondsTextSize}>s</size>";

            string scoreStr = string.Empty;
            for (int i = 0; i < _scoresString.Length; ++i)
                scoreStr += $"{_scoresString[i].ToColoredIf(_scoreHighlightColor, terminal.TrainingWorkshop.Tries > 0 && terminal.TrainingWorkshop.BestScore == i)}" +
                    $"{(i == _scoresString.Length - 1 ? "" : " ")}";
            _scoreText.text = scoreStr;

            TerminalScreenToggled?.Invoke(true);
        }

        private void ShutdownTerminalScreen()
        {
            ConsoleProLogger.Log(this, $"Shutting down Training Workshop terminal screen.", gameObject);
            _canvas.enabled = false;
            TerminalScreenToggled?.Invoke(false);
        }

        private void Awake()
        {
            for (int i = _terminals.Length - 1; i >= 0; --i)
                _terminals[i].Interacted += OnTerminalInteracted;

            _scoreBaseColor = _scoreText.color;
        }

        private void Update()
        {
            if (!_canvas.enabled)
                return;

            if (!Manager.ReferencesHub.FPSMaster.FPSInteracter.InteractedThisFrame && Input.GetButtonDown("Interact"))
                ShutdownTerminalScreen();
        }

        private void OnDestroy()
        {
            for (int i = _terminals.Length - 1; i >= 0; --i)
                _terminals[i].Interacted -= OnTerminalInteracted;
        }

#if UNITY_EDITOR
        [ContextMenu("Locate All Terminals")]
        private void LocateAllTerminals()
        {
            _terminals = FindObjectsOfType<TrainingWorkshopTerminal>();
            ConsoleProLogger.Log(this, $"Located {_terminals.Length} terminal(s).", gameObject);
        }
#endif
    }
}