namespace Doomlike.DialogueSystem
{
    using UnityEngine;

    public class DialoguesCallbacksHandler : MonoBehaviour, IConsoleProLoggable
    {
        [System.Serializable]
        public class DialogueCallback
        {
            [SerializeField] private string _dialogueId = string.Empty;
            [SerializeField] private UnityEngine.Events.UnityEvent _onDialogueTriggered = null;
            [SerializeField] private UnityEngine.Events.UnityEvent _onDialogueOver = null;

            public string DialogueId => _dialogueId;

            public bool DialogueTriggeredCallback()
            {
                _onDialogueTriggered?.Invoke();
                return _onDialogueTriggered.GetPersistentEventCount() > 0;
            }

            public bool DialogueOverCallback()
            {
                _onDialogueOver?.Invoke();
                return _onDialogueOver.GetPersistentEventCount() > 0;
            }
        }

        [SerializeField] private bool _consoleProMuted = false;
        [SerializeField] private DialogueCallback[] _dialoguesCallbacks = null;

        private System.Collections.Generic.Dictionary<string, DialogueCallback> _callbacksByIds;

        public bool ConsoleProMuted => _consoleProMuted;

        public string ConsoleProPrefix => "Dialogues Callbacks Handler";

        private void OnDialogueTriggered(ISentencesContainer dialogue)
        {
            if (_callbacksByIds.TryGetValue(dialogue.Id, out DialogueCallback callbackHandler))
                if (callbackHandler.DialogueTriggeredCallback())
                    this.Log($"Dialogue trigger callback executed for dialogue <b>{dialogue.Id}</b>.");
        }

        private void OnDialogueOver(ISentencesContainer dialogue)
        {
            if (_callbacksByIds.TryGetValue(dialogue.Id, out DialogueCallback callbackHandler))
                if (callbackHandler.DialogueOverCallback())
                    this.Log($"Dialogue over callback executed for dialogue <b>{dialogue.Id}</b>.");
        }

        private void Awake()
        {
            if (!Manager.ReferencesHub.TryGetDialogueController(out DialogueController dialogueCtrl))
            {
                this.LogError("Callbacks can not be registered if there's no instance of DialogueController in the scene.");
                return;
            }

            _callbacksByIds = new System.Collections.Generic.Dictionary<string, DialogueCallback>();
            for (int i = _dialoguesCallbacks.Length - 1; i >= 0; --i)
            {
                UnityEngine.Assertions.Assert.IsFalse(_callbacksByIds.ContainsKey(_dialoguesCallbacks[i].DialogueId), $"Duplicate Id for dialogue {_dialoguesCallbacks[i]}.");
                UnityEngine.Assertions.Assert.IsFalse(string.IsNullOrEmpty(_dialoguesCallbacks[i].DialogueId), "Can not register dialogue callbacks with a null or empty Id.");

                _callbacksByIds.Add(_dialoguesCallbacks[i].DialogueId, _dialoguesCallbacks[i]);
            }

            dialogueCtrl.DialogueTriggered += OnDialogueTriggered;
            dialogueCtrl.DialogueOver += OnDialogueOver;
        }

        private void OnDestroy()
        {
            if (!Manager.ReferencesHub.TryGetDialogueController(out DialogueController dialogueCtrl))
            {
                dialogueCtrl.DialogueTriggered -= OnDialogueTriggered;
                dialogueCtrl.DialogueOver -= OnDialogueOver;
            }
        }
    }
}