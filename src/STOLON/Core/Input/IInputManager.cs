
namespace STOLON
{
    public interface IInputManager
    {
        KeyboardInfo Keyboard { get; }
        MouseInfo Mouse { get; }
        IDrawable? MouseOn { get; }

        bool IsClicked(Keys key);
        bool IsClicked(MouseButton button);
        bool IsPressed(Keys key);
        bool IsPressed(MouseButton button);
        void Update(int elapsedMilliseconds);
        void PostUpdate(int elapsedMilliseconds);
        bool IsMouseOn<TElement>(TElement element) where TElement : class, IDrawable;
        bool IsMouseOn<TElement>() where TElement : class, IDrawable;

        [Obsolete] // Use the RegisterDraw() on DrawingContext instead.
        void RegisterDraw<TElement>(TElement element, in Rectangle hitbox) where TElement : class, IDrawable;
    }
}