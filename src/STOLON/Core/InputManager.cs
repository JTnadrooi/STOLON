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

    public class InputManager : Service
    {
        public InputManager() : base(null)
        {

        }

        public override void Update(int elapsedMilliseconds)
        {
            PreviousMouse = STOLON.Input.CurrentMouse;
            CurrentMouse = Mouse.GetState();

            if (!STOLON.Instance.GraphicsDevice.Viewport.Bounds.Contains(STOLON.Input.CurrentMouse.Position)) STOLON.Input.Domain = MouseDomain.None;
            else STOLON.Input.Domain = MouseDomain.OnScreen;

            STOLON.Input.PreviousKeyboard = STOLON.Input.CurrentKeyboard;
            STOLON.Input.CurrentKeyboard = Keyboard.GetState();
        }

        public MouseDomain Domain { get; private set; }

        public MouseFocus Focus { get; private set; }

        public MouseState PreviousMouse { get; private set; }

        public MouseState CurrentMouse { get; private set; }

        public KeyboardState CurrentKeyboard { get; private set; }

        public KeyboardState PreviousKeyboard { get; private set; }

        public Vector2 VirtualMousePos
            => Vector2.Transform(CurrentMouse.Position.ToVector2() - STOLON.DrawingContext.GameWindowDrawOffsetWithCorrectedY, STOLON.DrawingContext.InvertYMatrix) / STOLON.DrawingContext.Scale;

        public int MouseScrollValue => CurrentMouse.ScrollWheelValue;
        /// <summary>
        /// Change in scroll value since the last frame (positive = scrolled up, negative = down).
        /// </summary>
        public int MouseScrollDelta => CurrentMouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;
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
    }
}
