
namespace STOLON
{
    public interface IInputManager : IUpdatable
    {
        KeyboardState CurrentKeyboard { get; }
        MouseState CurrentMouse { get; }
        MouseDomain Domain { get; }
        MouseFocus Focus { get; }
        int MouseScrollDelta { get; }
        int MouseScrollValue { get; }
        KeyboardState PreviousKeyboard { get; }
        MouseState PreviousMouse { get; }
        Vector2 VirtualMousePos { get; }

        bool IsClicked(Keys key);
        bool IsClicked(MouseButton button);
        bool IsPressed(Keys key);
        bool IsPressed(MouseButton button);
        void SetCursor(MouseCursor cursor);
        void CollapseCursor();
    }
}