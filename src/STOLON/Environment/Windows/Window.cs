using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public readonly record struct WindowButtonClickArgs(Window Window);
    public readonly record struct WindowButtonHoverArgs(Window Window);

    [Flags]
    internal enum WindowButtonState
    {
        Enabled = 1,
        Hovered = 2,
    }


    public abstract class WindowButton
    {
        public Texture2D Texture { get; }

        private Window? _window;

        public const int Size = 9;

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

        protected Texture2D DefaultTexture { get; }
        protected Texture2D HoverTexture { get; }

        private WindowButtonState _state;

        protected WindowButton(Texture2D defaultTexture, Texture2D hoverTexture, int order)
        {
            ThrowIfInvalidTexture(defaultTexture);

            DefaultTexture = defaultTexture;
            HoverTexture = hoverTexture;

            Texture = defaultTexture;

            Order = order;
        }

        private void ThrowIfInvalidTexture(Texture2D texture)
        {
            if (texture.Width != Size || texture.Height != Size) throw new ArgumentException("Invalid texture dimensions.");
        }

        protected virtual void OnClick() { } // maybe make it so the Kernel doesnt create the ... args if this isnt overrriden? (reflection check)
        protected virtual void OnHover() { } // same here.

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

        internal void Click()
        {
            _state = _state | WindowButtonState.Enabled;

            Debug.Assert(HasWindow);

            OnClick();
        }

        internal void Hover()
        {
            _state = _state | WindowButtonState.Hovered;

            Debug.Assert(HasWindow);

            OnHover();
        }

        internal void SetState(WindowButtonState state)
        {
            _state = state;
        }
    }

    public sealed class CloseWindowButton : WindowButton
    {
        public CloseWindowButton(ITexture2DCollection textures) : base(textures["UI\\Window\\window_button_close"], textures["UI\\Window\\window_button_close"], 0)
        {

        }

        protected override void OnClick()
        {
            base.OnClick();
        }
    }

    public sealed class ToggleLockWindowButton : WindowButton
    {
        public ToggleLockWindowButton(ITexture2DCollection textures) : base(textures["UI\\Window\\window_button_lock"], textures["UI\\Window\\window_button_lock"], 1)
        {

        }

        protected override void OnClick()
        {
            base.OnClick();
        }
    }

    public abstract class Window : IComponent
    {
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public bool IsDraggable { get; protected set; }
        public bool IsResizable { get; protected set; }
        public bool IsBorderless { get; protected set; }

        /// <summary>
        /// Gets or sets whenever  <see cref="Update(int)"/> and <see cref="Draw(DrawingContext)"/> get called by the <see cref="Kernel"/>.
        /// </summary>
        public bool IsManaged { get; set; }

        public Rectangle InnerBounds
        {
            get => _innerBounds;
            set => _innerBounds = value;
        }

        public Rectangle OuterBounds
        {
            get
            {
                return new Rectangle(
                    _innerBounds.Location + new Point(-Border.PaddingLeft, -Border.PaddingBottom),
                    _innerBounds.Size + new Point(Border.AddedWidth, Border.AddedHeight)
                );
            }
            set
            {
                _innerBounds =
                    new Rectangle(
                        value.Location + new Point(Border.PaddingLeft, Border.PaddingBottom),
                        value.Size + new Point(-Border.AddedWidth, -Border.AddedHeight)
                    );
            }
        }

        //public Vector2 InnerPos
        //{
        //    get => _innerBounds.Location.ToVector2();
        //    set
        //    {
        //        _innerBounds.Location = value.ToPoint(); wont work
        //    }
        //}

        public Vector2 Position
        {
            get => OuterBounds.Location.ToVector2();
            set
            {
                OuterBounds = new Rectangle(value.ToPoint(), OuterBounds.Size);
            }
        }

        public Matrix TransformMatrix { get; private set; }
        public Border Border { get; }
        protected IReadOnlyDictionary<Type, WindowButton> Buttons => _buttons;

        private Rectangle _innerBounds;
        private TypeDictionary<WindowButton> _buttons;
        private WindowButton[] _orderedButtons;

        protected Window(Kernel kernel, ITexture2DCollection textures, int innerSizeX, int innerSizeY)
        {
            _textures = textures;
            _kernel = kernel;

            IsManaged = true;
            InnerBounds = new Rectangle(0, 0, innerSizeX, innerSizeY);
            Border = new Border(_textures["UI\\Window\\window-border"], 13, 4, 4, 4);

            kernel.RegisterWindow(this);

            _buttons = new TypeDictionary<WindowButton>();
            _orderedButtons = Array.Empty<WindowButton>();
        }

        private void UpdateButtons()
        {
            _orderedButtons = Buttons.Values.OrderBy(w => w.Order).ToArray();
        }

        protected void AddButton<TButton>(TButton button) where TButton : WindowButton
        {
            if (button.HasWindow) throw new InvalidOperationException($"Cannot add button to multiple windows. Button is already added to '{button.Window}'.");

            _buttons.Add(button);

            button.BindTo(this);

            UpdateButtons();
        }

        protected void RemoveButton<TButton>() where TButton : WindowButton
        {
            WindowButton button = _buttons.GetValue<TButton>();

            _buttons.Remove<TButton>();

            button.Unbind();

            UpdateButtons();
        }

        protected Vector2 ScreenToLocal(Point screenPosition) => ScreenToLocal(screenPosition.ToVector2());
        protected Vector2 ScreenToLocal(Vector2 screenPosition)
        {
            return screenPosition - InnerBounds.Location.ToVector2();
        }

        public void Offset(Vector2 amount)
        {

        }

        public virtual void Update(int elapsedMilliseconds)
        {
            TransformMatrix = Matrix.CreateTranslation(InnerBounds.Location.X, InnerBounds.Location.Y, 0);

            UpdateContents(elapsedMilliseconds);
        }

        protected virtual void UpdateContents(int elapsedMilliseconds) { }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawArea(InnerBounds, Color.Black);

            drawingContext.SetDrawingParameters(scissorArea: InnerBounds, transformMatrix: TransformMatrix);

            DrawContents(drawingContext);

            drawingContext.SetDrawingParameters(); // resets them.

            drawingContext.DrawBorderAround(Border, InnerBounds);

            for (int i = 0; i < _orderedButtons.Length; i++)
            {
                drawingContext.Draw(_orderedButtons[i].Texture,
                    OuterBounds.Location.ToVector2() + new Vector2(OuterBounds.Width - 4 - WindowButton.Size - (WindowButton.Size + 2) * i, OuterBounds.Height - WindowButton.Size - 2));
            }
        }

        protected abstract void DrawContents(DrawingContext drawingContext);
    }
}
