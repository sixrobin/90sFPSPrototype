namespace Doomlike.DialogueSystem
{
    using System.Collections;
    using UnityEngine;

    public class DialogueController : MonoBehaviour, IConsoleProLoggable
    {
        private const string SKIP_INPUT_NAME = "DialogueSkip";

        private ISentencesContainer _currDialogue;
        private ISentence _currSentence;

        private bool _skipPressed;

        public delegate void DialogueEventHandler(ISentencesContainer dialogue);

        public event DialogueEventHandler DialogueTriggered;
        public event DialogueEventHandler DialogueOver;

        [Header("UI REFERENCES")]
        [SerializeField] private Canvas _dialogueCanvas = null;
        [SerializeField] private GameObject _skipFeedback = null;
        [SerializeField] private TMPro.TextMeshProUGUI _speakerNameText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _sentenceText = null;

        [Header("APPEARANCE SETTINGS")]
        [SerializeField, Min(1)] private int _lettersPerTick = 3;
        [SerializeField, Min(0f)] private float _tickInterval = 0.1f;
        [SerializeField, Min(0f)] private float _skipButtonDelay = 1f;

        [Header("DEBUG")]
        [SerializeField] private bool _fastDialogues = false;
        [SerializeField] private bool _consoleProMuted = false;

        public bool DialoguePlaying { get; private set; }

        public bool ConsoleProMuted => _consoleProMuted;

        public string ConsoleProPrefix => "Dialogue System";

        public void Play(ISentencesContainer dialogue, GameObject context = null)
        {
            UnityEngine.Assertions.Assert.IsFalse(DialoguePlaying, "Trying to play a dialogue while another is playing.");
            this.Log($"Playing dialogue <b>{dialogue.Id}</b>.", context);

            _currDialogue = dialogue;

            ResetView();
            _dialogueCanvas.enabled = true;

            DialogueTriggered?.Invoke(_currDialogue);
            StartCoroutine(PlayDialogueCoroutine());
        }

        private bool SkipInputPressed()
        {
            return Input.GetButtonDown(SKIP_INPUT_NAME);
        }

        private void ResetView()
        {
            _sentenceText.text = string.Empty;
            _skipFeedback.SetActive(false);

            if (_speakerNameText != null)
            {
                if (_currSentence is NamedSentence namedSentence)
                {
                    _speakerNameText.text = namedSentence.SpeakerName;
                }
                else
                {
                    UnityEngine.Assertions.Assert.IsTrue(_currDialogue is OnePersonDialogue);
                    _speakerNameText.text = (_currDialogue as OnePersonDialogue).SpeakerName;
                }
            }
        }

        private void Skip()
        {
            _skipPressed = true;
            StartCoroutine(AvoidDoubleSkipInputCoroutine());
        }

        private IEnumerator PlayDialogueCoroutine()
        {
            DialoguePlaying = true;

            for (int i = 0; i < _currDialogue.Sentences.Length; ++i)
            {
                yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;

                _currSentence = _currDialogue.Sentences[i];
                yield return StartCoroutine(PlaySentenceCoroutine());
                yield return StartCoroutine(WaitForSkipCoroutine());
            }

            this.Log($"Dialogue over.");

            DialogueOver?.Invoke(_currDialogue);
            _currDialogue = null;

            DialoguePlaying = false;
            _dialogueCanvas.enabled = false;
        }

        private IEnumerator PlaySentenceCoroutine()
        {
            this.Log($"Playing next sentence.");

            ResetView();

            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
            string str = string.Empty;

            for (int i = 0; i < _currSentence.Text.Length; i += _lettersPerTick)
            {
                str = (i + _lettersPerTick > _currSentence.Text.Length) ? _currSentence.Text : str + _currSentence.Text.Substring(i, _lettersPerTick);
                _sentenceText.text = str;

                for (float t = 0; t < (_fastDialogues ? 0f : _tickInterval); t += Time.deltaTime)
                {
                    if (SkipInputPressed() && !_skipPressed)
                    {
                        _sentenceText.text = _currSentence.Text;
                        _skipFeedback.SetActive(true);

                        Skip();

                        yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
                        yield break;
                    }

                    yield return null;
                }
            }
        }

        private IEnumerator WaitForSkipCoroutine()
        {
            if (_fastDialogues)
                yield break;

            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;

            for (float t = 0f; t < _skipButtonDelay; t += Time.deltaTime)
            {
                if (SkipInputPressed() && !_skipPressed)
                {
                    Skip();
                    yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
                    yield break;
                }

                yield return null;
            }

            _skipFeedback.SetActive(true);
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;

            while (true)
            {
                if (SkipInputPressed() && !_skipPressed)
                {
                    Skip();
                    yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Used to make sure the skip input is considered only once per frame and that the dialogue plays normally.
        /// </summary>
        /// <returns></returns>
        private IEnumerator AvoidDoubleSkipInputCoroutine()
        {
            yield return RSLib.Yield.SharedYields.WaitForEndOfFrame;
            _skipPressed = false;
        }

        private void Awake()
        {
            Console.DebugConsole.OverrideCommand(new Console.DebugCommand("toggleFastDialogues", "Toggles dialogues light speed.", true, false, () =>
            {
                _fastDialogues = !_fastDialogues;
                ConsoleProLogger.Log(this, $"Fast dialogues {(_fastDialogues ? "on" : "off")}.", gameObject);
                Console.DebugConsole.LogExternal($"Fast dialogues {(_fastDialogues ? "on" : "off")}.");
            }));
        }
    }
}