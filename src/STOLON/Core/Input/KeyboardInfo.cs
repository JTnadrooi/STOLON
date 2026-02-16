namespace STOLON
{
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
}
