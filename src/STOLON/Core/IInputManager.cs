
namespace STOLON
{
    public interface IInputManager
    {
        KeyboardInfo Keyboard { get; }
        MouseInfo Mouse { get; }

        bool IsClicked(Keys key);
        bool IsClicked(MouseButton button);
        bool IsPressed(Keys key);
        bool IsPressed(MouseButton button);
        void Update(int elapsedMilliseconds);
        void PostUpdate(int elapsedMilliseconds);
    }
}