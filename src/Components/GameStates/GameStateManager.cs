using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Point = Microsoft.Xna.Framework.Point;
using Microsoft.Xna.Framework.Content;

namespace STOLON
{
    public interface IGameState
    {
        public void Update(int elapsedMilliseconds);
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds);
        public string DRPStatus => this.GetType().Name;
    }

    public static class GameStateExtensions
    {
        public static string GetId(this IGameState state) => GameStateHelpers.GetId(state.GetType());
    }
    public static class GameStateHelpers
    {
        public static string GetId<T>() where T : IGameState => GetId(typeof(T));
        public static string GetId(Type type) => type.FullName ?? throw new Exception();
    }
    public class GameStateManager
    {
        private IGameState? _currentState;
        private readonly Dictionary<string, IGameState> _stateMemory;

        public IGameState Current => _currentState ?? throw new Exception();

        public GameStateManager()
        {
            _stateMemory = new Dictionary<string, IGameState>();
        }

        public void ChangeState<T>(bool @override = false) where T : IGameState, new()
        {
            if (@override) _currentState = _stateMemory[GameStateHelpers.GetId<T>()] = new T();
            else _currentState = _stateMemory[GameStateHelpers.GetId<T>()] = _stateMemory.GetValueOrDefault(GameStateHelpers.GetId<T>()) ?? new T();
        }

        public void Update(int elapsedMilliseconds)
        {
            _currentState.Update(elapsedMilliseconds);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            _currentState.Draw(spriteBatch, elapsedMiliseconds);
        }

        public TGameState GetCurrent<TGameState>() where TGameState : IGameState => (TGameState)Current;
        public bool IsCurrent<TGameState>() where TGameState : IGameState => Current is TGameState;
        public bool TryGetState<TGameState>(out TGameState? state) where TGameState : IGameState
            => _stateMemory.TryGetValue(GameStateHelpers.GetId<TGameState>(), out var s) & (state = (TGameState?)s) != null;
    }
}