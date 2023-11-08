namespace Doomlike.DialogueSystem
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "New One Person Dialogue", menuName = "Doomlike/Dialogue/One Person Dialogue")]
    public class OnePersonDialogue : Dialogue
    {
        [SerializeField] private string _speakerName = string.Empty;
        [SerializeField] private Sentence[] _sentences = null;

        public string SpeakerName => _speakerName;

        public override ISentence[] Sentences => _sentences;
    }
}