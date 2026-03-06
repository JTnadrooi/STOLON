
namespace STOLON
{
    public interface ISceneManager
    {
        Scene Current { get; }
        string? SkipTarget { get; }

        void ChangeScene<T>() where T : Scene;
        void Draw(DrawingContext drawingContext);
        TScene GetCurrent<TScene>() where TScene : Scene;
        IReadOnlyList<string>? GetCurrentSkipParameters();
        IReadOnlyList<string>? GetSkipParameters(Scene scene);
        bool IsCurrent<TScene>() where TScene : Scene;
        bool IsCurrentSceneSkipTarget();
        bool IsSkipTarget(Scene scene);
        bool ShouldSkipAnimation(Scene scene);
        bool ShouldSkipCurrentAnimation();
        void Update(int elapsedMilliseconds);
    }
}