using System.Diagnostics;

namespace STOLON
{
    public abstract class WindowButton
    {
        private Texture2D _texture;

        public Texture2D Texture
        {
            get => _texture;
            protected set
            {
                ThrowIfInvalidTexture(value);

                _texture = value;
            }
        }

        protected Texture2D InitialTexture { get; }

        private Window? _window;

        public const int Size = 11;

        public Window Window
        {
            get => _window ?? throw new InvalidOperationException("This button is not bound to any window. Use Window.AddButton() to add buttons to a window.");
        }

        public bool HasWindow => _window is not null;

        /// <summary>
        /// Get the order of the button as placed in the topleft of the window. 
        /// Higher values put the button more left. The button position is calculated relative to the other buttons.
        /// </summary>
        public int Order { get; }

        protected WindowButton(Texture2D texture, int order)
        {
            ThrowIfInvalidTexture(texture);

            _texture = texture;

            InitialTexture = _texture;

            Order = order;
        }

        private void ThrowIfInvalidTexture(Texture2D texture)
        {
            if (texture.Width != Size || texture.Height != Size) throw new ArgumentException("Invalid texture dimensions.");
        }

        protected virtual void OnClick(Window source) { }
        protected virtual void OnHover(Window source) { }
        protected virtual void OnDefault(Window source) { }

        internal void BindTo(Window window)
        {
            Debug.Assert(!HasWindow, "BindTo should only be used once per WindowButton instance.");

            _window = window;
        }

        internal void Unbind()
        {
            Debug.Assert(HasWindow, "Cannot unbind already unbound (or never bound) window buttons.");

            _window = null;
        }

        internal void Click(Window source)
        {
            Debug.Assert(HasWindow);

            OnClick(source);
        }

        internal void Hover(Window source)
        {
            Debug.Assert(HasWindow);

            OnHover(source);
        }

        internal void Default(Window source)
        {
            Debug.Assert(HasWindow);

            OnDefault(source);
        }
    }
}
