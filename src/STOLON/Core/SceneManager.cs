using Autofac;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public abstract class Scene : IComponent
    {
        public string Id { get; }

        protected Scene(string id)
        {
            Id = id;
        }

        public string GetId() => GetId(this.GetType());
        public bool ShouldSkipAnimation() => IsSkipTarget() && SkipSceneAnimation;
        public bool IsSkipTarget() => SkipTarget == Id;
        public ReadOnlyCollection<string>? GetSkipParameters() => IsSkipTarget() ? SkipParameters : null;

        public void Update(int elapsedMilliseconds)
        {
            UpdateInterface(elapsedMilliseconds);
            UpdateContent(elapsedMilliseconds);
        }

        protected virtual void UpdateInterface(int elapsedMilliseconds) { }

        protected virtual void UpdateContent(int elapsedMilliseconds) { }

        public static string GetId<T>() where T : Scene => GetId(typeof(T));
        public static string GetId(Type type) => type.FullName ?? throw new Exception();

        public abstract void Draw(DrawingContext drawingContext);

        public static string SkipTarget { get; }
        public static bool SkipSceneAnimation { get; }
        public static ReadOnlyCollection<string> SkipParameters { get; }

        static Scene()
        {
            if (STOLON.IsInitiated)
            {
                Configuration config = STOLON.Services.Resolve<Configuration>();

                SkipTarget = config.GetString("debug.skip.target");
                SkipSceneAnimation = config.GetBool("debug.skip.skip_gamestage_animation");
                SkipParameters = config.Get<string[]>("debug.skip.parameters").AsReadOnly();
            }
            else
            {
                SkipTarget = string.Empty;
                SkipSceneAnimation = false;
                SkipParameters = Array.Empty<string>().AsReadOnly();
            }
        }
    }

    [Dependency(ServiceLifetime.Singleton)]
    public sealed class SceneManager : ISceneManager
    {
        private readonly IContainer _container;

        private Scene? _currentScene;

        public Scene Current => _currentScene ?? throw new Exception();

        public SceneManager(IContainer container)
        {
            _container = container;
        }

        public void ChangeScene<T>() where T : Scene
        {
            _currentScene = _container.Resolve<T>();
        }

        public void Update(int elapsedMilliseconds)
        {
            _currentScene.Update(elapsedMilliseconds);
        }

        public void Draw(DrawingContext drawingContext)
        {
            _currentScene.Draw(drawingContext);
        }

        public TScene GetCurrent<TScene>() where TScene : Scene => (TScene)Current;
        public bool IsCurrent<TScene>() where TScene : Scene => Current is TScene;
    }
}