namespace STOLON
{
    public sealed class MouseInfo : IUpdatable
    {
        private readonly Lazy<DrawingContext> _drawingContext;

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

        public MouseInfo(Lazy<DrawingContext> drawingContext)
        {
            _drawingContext = drawingContext;

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
            => Vector2.Transform(pos - _drawingContext.Value.GameWindowDrawOffsetWithCorrectedY, _drawingContext.Value.InvertYMatrix) / _drawingContext.Value.Scale;

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

        public int GetCoefficient()
        {
            int result = 0;

            if (IsClicked(MouseButton.Left)) result += 1;
            if (IsClicked(MouseButton.Right)) result += -1;

            return result;
        }
    }
}
