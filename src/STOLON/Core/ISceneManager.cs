namespace STOLON
{
    public interface ISceneManager : IComponent
    {
        Scene Current { get; }

        void ChangeScene<T>() where T : Scene;
        TScene GetCurrent<TScene>() where TScene : Scene;
        bool IsCurrent<TScene>() where TScene : Scene;
    }
}