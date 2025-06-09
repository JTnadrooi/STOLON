using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Debug;

using MonoGame.Extended;


using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using DiscordRPC;
using DiscordRPC.Events;
using System.Reflection;
using System.Linq;



namespace STOLON
{
    /// <summary>
    /// The enviroment of the <see cref="STOLON"/> game.
    /// </summary>
    public class GameEnvironment : GameComponent, IDialogueProvider
    {
        /// <summary>
        /// The <see cref="OverlayEngine"/>.
        /// </summary>
        public OverlayEngine Overlayer => _overlayer;
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> listing all <see cref="Entity"/> objects and their <see cref="Entity.Id"/>.
        /// </summary>
        public ReadOnlyDictionary<string, Entity> Entities => new ReadOnlyDictionary<string, Entity>(_entities);
        public string SymbolNotation => "Ev";
        public string Name => "Environment";

        public TaskHeap TaskHeap { get; }

        private UserInterface _userInterface;
        private OverlayEngine _overlayer;
        private Dictionary<string, Entity> _entities;
        private GameStateManager _gameStateManager;

        internal GameEnvironment() : base(null)
        {
            _entities = new Dictionary<string, Entity>();
            _userInterface = null!;
            _overlayer = null!;
            _gameStateManager = new GameStateManager();
            STOLON.StateManager = _gameStateManager;
            STOLON.StateManager.ChangeState<MenuGameState>();
            TaskHeap = new TaskHeap();
        }
        internal void Initialize()
        {
            STOLON.Debug.Log(">[s]initialising environment");
            STOLON.Debug.Log(">searching for entities");
            Entity[] entities = STOLON.Scan<Entity>();
            foreach (Entity entity in entities)
            {
                STOLON.Debug.Log($"found entity with id \"{entity.Id}\" and name \"{entity.Name}\".");
                RegisterEntity(entity);
            }
            STOLON.Debug.Success();

            _userInterface = new UserInterface();
            STOLON.UI = _userInterface;
            _userInterface.Initialize();


            _overlayer = new OverlayEngine();
            //StolonGame.Instance.AudioEngine.SetPlayList(new Playlist(
            //    "debug1",
            //    "debug2"
            //));
            STOLON.Debug.Success();
        }
        public override void Update(int elapsedMiliseconds)
        {
            TaskHeap.Update(elapsedMiliseconds);
            _userInterface.Update(elapsedMiliseconds);

            STOLON.StateManager.Update(elapsedMiliseconds);

            _userInterface.PostUpdate(elapsedMiliseconds);
            STOLON.Audio.Update(elapsedMiliseconds);
            STOLON.Instance.DRP.UpdateDetails(STOLON.StateManager.Current.DRPStatus);

            _overlayer.Update(elapsedMiliseconds);
            base.Update(elapsedMiliseconds);
        }
        public override void Draw(DrawingContext drawingContext, int elapsedMiliseconds)
        {
            STOLON.StateManager.Draw(drawingContext, elapsedMiliseconds);
            _userInterface.Draw(drawingContext, elapsedMiliseconds);

            _overlayer.Draw(drawingContext, elapsedMiliseconds);
            base.Draw(drawingContext, elapsedMiliseconds);
        }

        /// <summary>
        /// Register a new <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The entity to register.</param>
        public void RegisterEntity(Entity entity)
        {
            _entities.Add(entity.Id, entity);
        }
        /// <summary>
        /// Deregister a new <see cref="Entity"/>. <strong>Should never be used.</strong>
        /// </summary>
        /// <param name="entity">The entity to deregister.</param>
        public void DeregisterEntity(string characterId)
        {
            _entities.Remove(characterId);
        }

        /// <summary>
        /// The main StolonGame.Instance of the game.
        /// </summary>
        public static GameEnvironment Instance => STOLON.Environment;
    }
}
