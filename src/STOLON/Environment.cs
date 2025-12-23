namespace STOLON
{
    /// <summary>
    /// The enviroment of the <see cref="STOLON"/> game.
    /// </summary>
    public class Environment : Service, IDialogueProvider, ISingletonDependency
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> listing all <see cref="Entity"/> objects and their <see cref="Entity.Id"/>.
        /// </summary>
        public ReadOnlyDictionary<string, Entity> Entities => new ReadOnlyDictionary<string, Entity>(_entityDict);
        public string SymbolNotation => "Ev";
        public string Name => "Environment";

        private Dictionary<string, Entity> _entityDict;

        private readonly IRichLogger _logger;
        private readonly IAudioEngine _audioEngine;
        private readonly ISceneManager _sceneManager;
        private readonly IOverlayManager _overlayManager;
        private readonly Interface _ui;
        private readonly IEnumerable<Entity> _entities;

        public Environment(IRichLogger logger, IAudioEngine audioEngine, ISceneManager sceneManager, Interface ui, IOverlayManager overlayManager, IEnumerable<Entity> entities) : base(null)
        {
            _logger = logger;
            _audioEngine = audioEngine;
            _sceneManager = sceneManager;
            _overlayManager = overlayManager;
            _ui = ui;
            _entities = entities;

            _entityDict = new Dictionary<string, Entity>();
            _sceneManager = sceneManager;
        }
        public void Initialize()
        {
            _logger.Log(">[s]initialising environment");
            _logger.Log(">searching for entities");
            foreach (Entity entity in _entities)
            {
                _logger.Log($"found entity with id '{entity.Id}\" and name '{entity.Name}\".");
                RegisterEntity(entity);
            }
            _logger.Success();

            _sceneManager.ChangeScene<MenuScene>();

            //StolonGame.Instance.AudioEngine.SetPlayList(new Playlist(
            //    "debug1",
            //    "debug2"
            //));
            _logger.Success();
        }
        public override void Update(int elapsedMilliseconds)
        {
            _ui.Update(elapsedMilliseconds);

            _sceneManager.Update(elapsedMilliseconds);

            //_userInterface.PostUpdate(elapsedMilliseconds);
            _audioEngine.Update(elapsedMilliseconds);
            //STOLON.Instance.DRP.UpdateDetails(STOLON.SceneManager.Current.DRPStatus);

            _overlayManager.Update(elapsedMilliseconds);
            base.Update(elapsedMilliseconds);
        }
        public override void Draw(DrawingContext drawingContext)
        {
            _sceneManager.Draw(drawingContext);
            _ui.Draw(drawingContext);

            _overlayManager.Draw(drawingContext);
            base.Draw(drawingContext);
        }

        /// <summary>
        /// Register a new <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The entity to register.</param>
        public void RegisterEntity(Entity entity)
        {
            _entityDict.Add(entity.Id, entity);
        }
        /// <summary>
        /// Deregister a new <see cref="Entity"/>. <strong>Should never be used.</strong>
        /// </summary>
        /// <param name="entity">The entity to deregister.</param>
        public void DeregisterEntity(string characterId)
        {
            _entityDict.Remove(characterId);
        }

        public Entity GetEntityInstance<TEntity>() => Entities.First(kvp => kvp.Value is TEntity).Value;
    }
}
