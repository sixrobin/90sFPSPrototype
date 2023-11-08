namespace Doomlike.DialogueSystem
{
    using UnityEngine;

    public abstract class Dialogue : ScriptableObject, ISentencesContainer
    {
        [SerializeField] private string _id = string.Empty;
        [SerializeField] private Dialogue _followingDialogue = null;

        public string Id => _id;

        public Dialogue FollowingDialogue => _followingDialogue;

        public abstract ISentence[] Sentences { get; }
    }
}