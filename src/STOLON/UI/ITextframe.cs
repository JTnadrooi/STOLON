
namespace STOLON
{
    public interface ITextframe
    {
        Rectangle DialogueBounds { get; }
        bool Hide { get; set; }

        void Draw(DrawingContext drawingContext);
        int GetMillisecondsFromText(string text);
        void Next();
        void Queue(DialogueInfo dialogue);
        void Queue(DialogueInfo[] dialogue);
        void Update(int elapsedMilliseconds);
    }
}