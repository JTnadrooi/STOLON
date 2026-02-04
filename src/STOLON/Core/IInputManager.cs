
namespace STOLON
{
    public interface IInputManager
    {
        KeyboardInfo Keyboard { get; }
        MouseInfo Mouse { get; }
        IDrawable? MouseFocus { get; }

        bool IsClicked(Keys key);
        bool IsClicked(MouseButton button);
        bool IsPressed(Keys key);
        bool IsPressed(MouseButton button);
        void Update(int elapsedMilliseconds);
        void PostUpdate(int elapsedMilliseconds);
        bool IsMouseFocus<TElement>(TElement element) where TElement : class, IDrawable;
        bool IsMouseFocus<TElement>() where TElement : class, IDrawable;
        void RegisterDraw<TElement>(TElement element, in Rectangle hitbox) where TElement : class, IDrawable;
    }
}