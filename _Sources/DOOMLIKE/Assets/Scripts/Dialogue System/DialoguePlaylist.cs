namespace Doomlike.DialogueSystem
{
    public class DialoguePlaylist
    {
        private Dialogue _nextDialogue;

        public DialoguePlaylist(Dialogue initDialogue)
        {
            _nextDialogue = initDialogue;
        }

        public Dialogue Next()
        {
            Dialogue next = _nextDialogue;
            _nextDialogue = _nextDialogue.FollowingDialogue ?? _nextDialogue;

            return next;
        }

        public void OverrideNextDialogue(Dialogue nextDialogue)
        {
            _nextDialogue = nextDialogue;
        }
    }
}