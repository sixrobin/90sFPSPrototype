namespace Doomlike.DialogueSystem
{
    using UnityEngine;

    [System.Serializable]
    public class Sentence : ISentence
    {
        [SerializeField, TextArea(3, 10)] private string _text = "";
        [SerializeField] private AudioClip _clip = null;

        public string Text => _text;

        public AudioClip Clip => _clip;
    }
}