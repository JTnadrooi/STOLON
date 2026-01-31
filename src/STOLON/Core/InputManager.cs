namespace STOLON
{
    public enum MouseDomain
    {
        None,
        OnScreen,
    }

    public enum MouseFocus
    {
        None,
        Textbox,
    }

    public enum MouseButton
    {
        Left,
        Middle,
        Right,
    }

    public class InputManager : IInputManager, ISingletonDependency
    {
        public MouseCursor Cursor { get; private set; }

        public MouseDomain Domain { get; private set; }

        public MouseFocus Focus { get; private set; }

        public MouseState PreviousMouse { get; private set; }

        public MouseState CurrentMouse { get; private set; }

        public KeyboardState CurrentKeyboard { get; private set; }

        public KeyboardState PreviousKeyboard { get; private set; }

        public Vector2 VirtualMousePos { get; private set; }

        public int MouseScrollValue => CurrentMouse.ScrollWheelValue;
        /// <summary>
        /// Change in scroll value since the last frame (positive = scrolled up, negative = down).
        /// </summary>
        public int MouseScrollDelta => CurrentMouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;

        private MouseCursor? _pendingCursor;

        public Vector2 MouseDelta { get; private set; }

        public InputManager()
        {
            Cursor = MouseCursor.Arrow;
        }

        private Vector2 _previousVirtualMousePos;

        public void Update(int elapsedMilliseconds)
        {
            _pendingCursor = null;

            SetCursor(MouseCursor.Arrow);

            PreviousMouse = CurrentMouse;
            CurrentMouse = Mouse.GetState();

            _previousVirtualMousePos = VirtualMousePos;
            VirtualMousePos = TransformMousePos(CurrentMouse.Position.ToVector2());
            MouseDelta = (VirtualMousePos - _previousVirtualMousePos);
            //MouseDelta = (CurrentMouse.Position.ToVector2() - PreviousMouse.Position.ToVector2()) / STOLON.DrawingContext.Scale * new Vector2(1, -1);
            //MouseDelta = TransformMousePos(MouseDelta);

            Console.WriteLine(MouseDelta);

            if (!STOLON.Instance.GraphicsDevice.Viewport.Bounds.Contains(CurrentMouse.Position)) Domain = MouseDomain.None;
            else Domain = MouseDomain.OnScreen;

            PreviousKeyboard = CurrentKeyboard;
            CurrentKeyboard = Keyboard.GetState();
        }

        private Vector2 TransformMousePos(Vector2 pos)
            => Vector2.Transform(pos - STOLON.DrawingContext.GameWindowDrawOffsetWithCorrectedY, STOLON.DrawingContext.InvertYMatrix) / STOLON.DrawingContext.Scale;


        public void CollapseCursor()
        {
            if (_pendingCursor is null || _pendingCursor == Cursor) return;

            Mouse.SetCursor(_pendingCursor);

            Cursor = _pendingCursor;
        }

        public bool IsPressed(MouseButton button) => IsPressed(CurrentMouse, button);
        private bool IsPressed(MouseState state, MouseButton button) => button switch
        {
            MouseButton.Left => state.LeftButton,
            MouseButton.Middle => state.MiddleButton,
            MouseButton.Right => state.RightButton,
            _ => throw new Exception(),
        } == ButtonState.Pressed;

        private bool IsPressed(KeyboardState state, Keys key) => state.IsKeyDown(key);
        public bool IsPressed(Keys key) => IsPressed(CurrentKeyboard, key);

        public bool IsClicked(Keys key) => IsPressed(CurrentKeyboard, key) && !IsPressed(PreviousKeyboard, key);
        public bool IsClicked(MouseButton button) => IsPressed(CurrentMouse, button) && !IsPressed(PreviousMouse, button);

        public void SetCursor(MouseCursor cursor)
        {
            ArgumentNullException.ThrowIfNull(cursor);

            _pendingCursor = cursor;
        }
    }
}
