namespace Doomlike.DialogueSystem
{
    using UnityEngine;

    [System.Serializable]
    public class NamedSentence : Sentence
    {
        [SerializeField] private string _speakerName = string.Empty;

        public string SpeakerName => _speakerName;
    }
}