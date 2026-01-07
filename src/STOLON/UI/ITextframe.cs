
namespace STOLON
{
    public interface ITextframe : IComponent
    {
        Rectangle DialogueBounds { get; }
        bool Hide { get; set; }

        int GetMillisecondsFromText(string text);
        void Next();
        void Queue(DialogueInfo dialogue);
        void Queue(DialogueInfo[] dialogue);
    }
}