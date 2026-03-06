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

        public void Update(int elapsedMilliseconds)
        {
            UpdateInterface(elapsedMilliseconds);
            UpdateContent(elapsedMilliseconds);
        }

        protected virtual void UpdateInterface(int elapsedMilliseconds) { }

        protected virtual void UpdateContent(int elapsedMilliseconds) { }

        public abstract void Draw(DrawingContext drawingContext);
    }

    [Dependency(ServiceLifetime.Singleton)]
    public sealed class SceneManager : ISceneManager
    {
        private readonly IContainer _container;
        private readonly Configuration _configuration;

        private readonly bool _skipSceneAnimation;
        private readonly IReadOnlyList<string>? _skipParameters;

        private Scene? _currentScene;
        public Scene Current => _currentScene ?? throw new Exception();

        private readonly string? _skipTarget;
        public string? SkipTarget => _skipTarget;

        public SceneManager(IContainer container, Configuration configuration)
        {
            _container = container;
            _configuration = configuration;

            _skipTarget = _configuration.GetString("debug.skip.target");
            _skipSceneAnimation = _configuration.GetBool("debug.skip.skip_gamestage_animation");
            _skipParameters = _configuration.Get<string[]>("debug.skip.parameters");
        }

        public void ChangeScene<T>() where T : Scene
        {
            _currentScene = _container.Resolve<T>();
        }

        public void Update(int elapsedMilliseconds)
        {
            _currentScene?.Update(elapsedMilliseconds);
        }

        public void Draw(DrawingContext drawingContext)
        {
            _currentScene?.Draw(drawingContext);
        }

        public TScene GetCurrent<TScene>() where TScene : Scene => (TScene)Current;

        public bool IsCurrent<TScene>() where TScene : Scene => Current is TScene;

        public bool ShouldSkipAnimation(Scene scene)
        {
            return IsSkipTarget(scene) && _skipSceneAnimation;
        }

        public bool IsSkipTarget(Scene scene)
        {
            return _skipTarget == scene.Id;
        }

        public IReadOnlyList<string>? GetSkipParameters(Scene scene)
        {
            return IsSkipTarget(scene) ? _skipParameters : null;
        }

        public bool IsCurrentSceneSkipTarget() => _currentScene is not null && IsSkipTarget(_currentScene);

        public bool ShouldSkipCurrentAnimation() => _currentScene is not null && ShouldSkipAnimation(_currentScene);

        public IReadOnlyList<string>? GetCurrentSkipParameters() =>
            _currentScene is not null ? GetSkipParameters(_currentScene) : null;
    }
}