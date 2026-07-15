using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace STOLON
{
    public enum MouseButton
    {
        Left,
        Middle,
        Right,
        XButton1,
        XButton2,
    }

    [Dependency(ServiceLifetime.Singleton)]
    public sealed class InputManager : IInputManager
    {
        private readonly record struct DrawableHitboxInfo(IDrawable Drawn, in Rectangle Hitbox);

        public KeyboardInfo Keyboard { get; }
        public MouseInfo Mouse { get; }

        public IDrawable? MouseOn => _mouseFocus;

        private readonly List<DrawableHitboxInfo> _drawnDrawables;
        private DrawableHitboxInfo[] _lastFrameDrawables;
        private IDrawable? _mouseFocus;

        private const int MaxDrawables = 128;

        public InputManager(Lazy<DrawingContext> drawingContext)
        {
            Keyboard = new KeyboardInfo();
            Mouse = new MouseInfo(drawingContext);

            _drawnDrawables = new List<DrawableHitboxInfo>(64);
            _lastFrameDrawables = Array.Empty<DrawableHitboxInfo>();
        }

        public void Update(int elapsedMilliseconds)
        {
            if (_drawnDrawables.Count > 0)
            {
                if (_lastFrameDrawables.Length < _drawnDrawables.Count)
                {
                    if (_drawnDrawables.Count < MaxDrawables)
                        _lastFrameDrawables = new DrawableHitboxInfo[_drawnDrawables.Count];
                    else throw new InvalidOperationException($"Cannot have more than {MaxDrawables} IDrawable's registered in one frame.");
                }

                for (int i = 0, j = _drawnDrawables.Count - 1; i < _drawnDrawables.Count; i++, j--)
                {
                    _lastFrameDrawables[i] = _drawnDrawables[j];
                }

                _drawnDrawables.Clear();
            }
            else
            {
                _lastFrameDrawables = Array.Empty<DrawableHitboxInfo>();
            }

            Keyboard.Update(elapsedMilliseconds);
            Mouse.Update(elapsedMilliseconds);
        }

        public void PostUpdate(int elapsedMilliseconds)
        {
            Mouse.CollapseCursor();

            _mouseFocus = null;

            for (int i = 0; i < _lastFrameDrawables.Length; i++)
            {
                ref readonly DrawableHitboxInfo item = ref _lastFrameDrawables[i];

                if (item.Hitbox.Contains(Mouse.Position))
                {
                    _mouseFocus = item.Drawn;
                    break;
                }
            }
        }

        public bool IsPressed(MouseButton button) => Mouse.IsPressed(button);
        public bool IsPressed(Keys key) => Keyboard.IsPressed(key);

        public bool IsClicked(MouseButton button) => Mouse.IsClicked(button);
        public bool IsClicked(Keys key) => Keyboard.IsClicked(key);

        public void RegisterDraw<TElement>(TElement element, in Rectangle hitbox) where TElement : class, IDrawable
        {
            _drawnDrawables.Add(new DrawableHitboxInfo(element, hitbox));
        }

        public bool IsMouseOn<TElement>() where TElement : class, IDrawable
        {
            return _mouseFocus is TElement;
        }

        public bool IsMouseOn<TElement>(TElement element) where TElement : class, IDrawable
        {
            return ReferenceEquals(element, _mouseFocus);
        }

        public bool IsMouseOn<TElement>(Func<TElement, bool> predicate) where TElement : class, IDrawable
        {
            return IsMouseOn<TElement>() && predicate.Invoke((TElement)_mouseFocus!);
        }
    }
}
