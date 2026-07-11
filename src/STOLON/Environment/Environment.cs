namespace STOLON
{
    /// <summary>
    /// The enviroment of the <see cref="STOLON"/> game.
    /// </summary>
    [Dependency(ServiceLifetime.Singleton)]
    public class Environment : IComponent, IDialogueProvider
    {
        private readonly IRichLogger _logger;
        private readonly IAudioEngine _audioEngine;
        private readonly ISceneManager _sceneManager;
        private readonly IOverlayManager _overlayManager;
        private readonly Interface _ui;
        private readonly IEnumerable<EntityDefinition> _entities;
        private readonly Kernel _kernel;

        public string SymbolNotation => "Ev";
        public string Name => "Environment";

        public Environment(IRichLogger logger,
            IAudioEngine audioEngine,
            ISceneManager sceneManager,
            Interface ui,
            IOverlayManager overlayManager,
            Kernel kernel,
            IEnumerable<EntityDefinition> entities)
        {
            _logger = logger;
            _audioEngine = audioEngine;
            _sceneManager = sceneManager;
            _overlayManager = overlayManager;
            _ui = ui;
            _entities = entities;
            _kernel = kernel;

            _sceneManager = sceneManager;
        }

        public void Initialize()
        {
            _logger.Log(">[s]initialising environment");
            foreach (EntityDefinition entity in _entities)
            {
                _logger.Log($"registered entity with id '{entity.Id}\" and name '{entity.Name}\".");
            }

            _sceneManager.ChangeScene<ShellScene>();

            //StolonGame.Instance.AudioEngine.SetPlayList(new Playlist(
            //    "debug1",
            //    "debug2"
            //));
            _logger.Success();
        }

        public void Update(int elapsedMilliseconds)
        {
            _ui.Update(elapsedMilliseconds);
            _sceneManager.Update(elapsedMilliseconds);
            _kernel.Update(elapsedMilliseconds);
            _audioEngine.Update(elapsedMilliseconds);
            _overlayManager.Update(elapsedMilliseconds);
        }

        public void Draw(DrawingContext drawingContext)
        {
            _sceneManager.Draw(drawingContext);
            _kernel.Draw(drawingContext);
            _ui.Draw(drawingContext);

            _overlayManager.Draw(drawingContext);
        }
    }
}
