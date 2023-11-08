namespace Doomlike.DialogueSystem
{
    public interface ISentencesContainer
    {
        string Id { get; }

        ISentence[] Sentences { get; }
    }
}