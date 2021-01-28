namespace Doomlike.DialogueSystem
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "New Multiple Person Dialogue", menuName = "Doomlike/Dialogue/Multiple Person Dialogue")]
    public class MultiplePersonDialogue : Dialogue
    {
        [SerializeField] private NamedSentence[] _sentences = null;

        public override ISentence[] Sentences => _sentences;
    }
}