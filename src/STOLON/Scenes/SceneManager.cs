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
    public abstract class Scene
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
            SkipTarget = STOLON.Config.GetString("debug.skip.target");
            SkipSceneAnimation = STOLON.Config.GetBool("debug.skip.skip_gamestage_animation");
            SkipParameters = STOLON.Config.Get<string[]>("debug.skip.parameters").AsReadOnly();
        }
    }

    public sealed class SceneManager
    {
        private Scene? _currentScene;
        private readonly Dictionary<string, Scene> _sceneMemory;

        public Scene Current => _currentScene ?? throw new Exception();

        public SceneManager()
        {
            _sceneMemory = new Dictionary<string, Scene>();
        }

        public void ChangeScene<T>(bool @override = false) where T : Scene, new()
        {
            if (@override) _currentScene = _sceneMemory[Scene.GetId<T>()] = new T();
            else _currentScene = _sceneMemory[Scene.GetId<T>()] = _sceneMemory.GetValueOrDefault(Scene.GetId<T>()) ?? new T();
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
        public bool TryGetState<TScene>(out TScene? state) where TScene : Scene
            => _sceneMemory.TryGetValue(Scene.GetId<TScene>(), out var s) & (state = (TScene?)s) != null;
    }
}