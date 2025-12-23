namespace STOLON
{
    public interface ISceneManager
    {
        Scene Current { get; }

        void ChangeScene<T>() where T : Scene;
        void Draw(DrawingContext drawingContext);
        TScene GetCurrent<TScene>() where TScene : Scene;
        bool IsCurrent<TScene>() where TScene : Scene;
        void Update(int elapsedMilliseconds);
    }
}