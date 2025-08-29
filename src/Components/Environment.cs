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
using System.Diagnostics;



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


        private Interface _userInterface;
        private OverlayEngine _overlayer;
        private Dictionary<string, Entity> _entities;
        private GameStateManager _stateManager;

        public GameEnvironment() : base(null)
        {
            _entities = new Dictionary<string, Entity>();
            _userInterface = null!;
            _stateManager = null!;
            _overlayer = null!;


        }
        public void Initialize()
        {
            STOLON.Debug.Log(">[s]initialising environment");
            STOLON.Debug.Log(">searching for entities");
            Entity[] entities = STOLON.Scan<Entity>();
            foreach (Entity entity in entities)
            {
                STOLON.Debug.Log($"found entity with id '{entity.Id}\" and name '{entity.Name}\".");
                RegisterEntity(entity);
            }
            STOLON.Debug.Success();

            STOLON.UI = _userInterface = new Interface();
            STOLON.StateManager = _stateManager = new GameStateManager();
            STOLON.StateManager.ChangeState<MenuGameState>();


            _overlayer = new OverlayEngine();
            //StolonGame.Instance.AudioEngine.SetPlayList(new Playlist(
            //    "debug1",
            //    "debug2"
            //));
            STOLON.Debug.Success();
        }
        public override void Update(int elapsedMilliseconds)
        {
            _userInterface.Update(elapsedMilliseconds);

            STOLON.StateManager.Update(elapsedMilliseconds);

            //_userInterface.PostUpdate(elapsedMilliseconds);
            STOLON.Audio.Update(elapsedMilliseconds);
            STOLON.Instance.DRP.UpdateDetails(STOLON.StateManager.Current.DRPStatus);

            _overlayer.Update(elapsedMilliseconds);
            base.Update(elapsedMilliseconds);
        }
        public override void Draw(DrawingContext drawingContext)
        {
            STOLON.StateManager.Draw(drawingContext);
            _userInterface.Draw(drawingContext);

            _overlayer.Draw(drawingContext);
            base.Draw(drawingContext);
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

        public Entity GetEntityInstance<TEntity>() => Entities.First(kvp => kvp.Value is TEntity).Value;
    }
}
