using Autofac;

namespace STOLON
{
    public abstract class Scene : ISingletonDependency
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
            UpdateUI(elapsedMilliseconds);
            UpdateEnvironment(elapsedMilliseconds);
        }

        protected virtual void UpdateUI(int elapsedMilliseconds) { }

        protected virtual void UpdateEnvironment(int elapsedMilliseconds) { }

        public abstract void Draw(DrawingContext drawingContext);

        public static string GetId<T>() where T : Scene => GetId(typeof(T));
        public static string GetId(Type type) => type.FullName ?? throw new Exception();

        public static string SkipTarget { get; }
        public static bool SkipSceneAnimation { get; }
        public static ReadOnlyCollection<string> SkipParameters { get; }

        static Scene()
        {
            Configuration config = STOLON.Services.Resolve<Configuration>();

            SkipTarget = config.GetString("debug.skip.target");
            SkipSceneAnimation = config.GetBool("debug.skip.skip_gamestage_animation");
            SkipParameters = config.Get<string[]>("debug.skip.parameters").AsReadOnly();
        }
    }

    public sealed class SceneManager : ISceneManager, ISingletonDependency
    {
        private Scene? _currentScene;

        public Scene Current => _currentScene ?? throw new Exception();

        public SceneManager()
        {

        }

        public void ChangeScene<T>() where T : Scene
        {
            _currentScene = STOLON.Services.Resolve<T>();
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