using DiscordRPC;

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

    public sealed class MouseInfo : IUpdatable
    {
        private MouseState _currentState;
        private MouseState _previousState;
        private Vector2 _previousPos;
        private bool _isOnScreen;
        private MouseCursor? _pendingCursor;

        public MouseCursor Cursor { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Delta { get; private set; }
        public int ScrollValue { get; private set; }

        /// <summary>
        /// Gets the change in scroll value since the last frame (positive = scrolled up, negative = down).
        /// </summary>
        public int ScrollDelta { get; private set; }

        public MouseInfo()
        {
            Cursor = MouseCursor.Arrow;
        }

        private static bool IsPressedImpl(in MouseState state, in MouseButton button) => button switch
        {
            MouseButton.Left => state.LeftButton,
            MouseButton.Middle => state.MiddleButton,
            MouseButton.Right => state.RightButton,
            MouseButton.XButton1 => state.XButton1,
            MouseButton.XButton2 => state.XButton2,
            _ => throw new InvalidOperationException(),
        } == ButtonState.Pressed;

        private Vector2 TransformMousePos(Vector2 pos)
            => Vector2.Transform(pos - STOLON.DrawingContext.GameWindowDrawOffsetWithCorrectedY, STOLON.DrawingContext.InvertYMatrix) / STOLON.DrawingContext.Scale;

        public void SetCursor(MouseCursor cursor)
        {
            ArgumentNullException.ThrowIfNull(cursor);

            _pendingCursor = cursor;
        }

        public void CollapseCursor()
        {
            if (_pendingCursor is null || _pendingCursor == Cursor) return;

            Mouse.SetCursor(_pendingCursor);

            Cursor = _pendingCursor;
        }

        public void Update(int elapsedMilliseconds)
        {
            _pendingCursor = null;

            SetCursor(MouseCursor.Arrow);

            _previousState = _currentState;
            _currentState = Mouse.GetState();

            _previousPos = Position;
            Position = TransformMousePos(_currentState.Position.ToVector2());
            Delta = (Position - _previousPos);

            ScrollDelta = _currentState.ScrollWheelValue - _previousState.ScrollWheelValue;

            _isOnScreen = !STOLON.Instance.GraphicsDevice.Viewport.Bounds.Contains(_currentState.Position);
        }

        public bool IsPressed(MouseButton button) => IsPressedImpl(_currentState, button);

        public bool IsClicked(MouseButton button) => IsPressedImpl(_currentState, button) && !IsPressedImpl(_previousState, button);
    }

    public sealed class KeyboardInfo : IUpdatable
    {
        private KeyboardState _currentState;
        private KeyboardState _previousState;

        public KeyboardInfo() { }

        private static bool IsPressedImpl(in KeyboardState state, in Keys key) => state.IsKeyDown(key);

        public void Update(int elapsedMilliseconds)
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();
        }

        public bool IsPressed(Keys key) => IsPressedImpl(_currentState, key);

        public bool IsClicked(Keys key) => IsPressedImpl(_currentState, key) && !IsPressedImpl(_previousState, key);
    }

    [Dependency(ServiceLifetime.Singleton)]
    public class InputManager : IInputManager
    {
        public KeyboardInfo Keyboard { get; }
        public MouseInfo Mouse { get; }

        public InputManager()
        {
            Keyboard = new KeyboardInfo();
            Mouse = new MouseInfo();
        }

        public void Update(int elapsedMilliseconds)
        {
            Keyboard.Update(elapsedMilliseconds);
            Mouse.Update(elapsedMilliseconds);
        }

        public void PostUpdate(int elapsedMilliseconds)
        {
            Mouse.CollapseCursor();
        }

        public bool IsPressed(MouseButton button) => Mouse.IsPressed(button);
        public bool IsPressed(Keys key) => Keyboard.IsPressed(key);

        public bool IsClicked(MouseButton button) => Mouse.IsClicked(button);
        public bool IsClicked(Keys key) => Keyboard.IsClicked(key);
    }
}
