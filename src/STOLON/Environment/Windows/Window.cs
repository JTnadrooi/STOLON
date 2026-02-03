using Microsoft.Xna.Framework.Graphics;
using STOLON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public abstract class Window : IComponent, IPositionable
    {
        private readonly record struct WindowButtonDrawInfo(WindowButton Button, Rectangle Bounds);

        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;
        private readonly IFont2DCollection _fonts;
        private readonly IInputManager _input;

        public bool IsDraggable { get; set; }
        public bool IsResizable { get; set; }
        public bool IsBorderless { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Gets the <see cref="WindowShellRegion"/> bound to this <see cref="Window"/> instance or <see langword="null"/> if this window is not bound to any region.
        /// </summary>
        public WindowShellRegion? BoundRegion
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets if this <see cref="Window"/> is currently locked to the <see cref="BoundRegion"/>. 
        /// Locked windows are always managed by the <see cref="BoundRegion"/> instead of the <see cref="Kernel"/>. 
        /// (When this is <see langword="true"/>, <see cref="IsManaged"/> is always <see langword="false"/>.)
        /// </summary>
        public bool IsLocked => BoundRegion is not null && BoundRegion.IsLockActive();

        private bool _isManaged;

        /// <summary>
        /// Gets or sets whenever  <see cref="Update(int)"/> and <see cref="Draw(DrawingContext)"/> get called by the <see cref="Kernel"/>.
        /// </summary>
        public bool IsManaged
        {
            get => _isManaged;
            set
            {
                if (IsLocked) throw new InvalidOperationException("Cannot change IsManaged for locked window.");

                _isManaged = value;
            }
        }

        public Rectangle InnerBounds
        {
            get => _innerBounds;
            set
            {
                _innerBounds = value;

                UpdatePosition();
            }
        }

        public Rectangle OuterBounds
        {
            get
            {
                return new Rectangle(
                    InnerBounds.Location + new Point(-Border.PaddingLeft, -Border.PaddingBottom),
                    InnerBounds.Size + new Point(Border.AddedWidth, Border.AddedHeight)
                );
            }
            set
            {
                InnerBounds =
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

        public Vector2 Position // could be optimised by offsetting rectangles instead.
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
        private WindowButtonDrawInfo[] _orderedButtons;
        private Font2D _nameFont;

        const int spacing = 2;

        protected Window(Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, int innerSizeX, int innerSizeY)
        {
            _textures = textures;
            _kernel = kernel;
            _fonts = fonts;
            _input = input;
            _nameFont = fonts.Medium;

            _buttons = new TypeDictionary<WindowButton>();
            _orderedButtons = Array.Empty<WindowButtonDrawInfo>();

            IsManaged = true;
            Border = new Border(_textures["UI\\Window\\window-border"], 15, 1, 1, 1);
            Name = string.Empty;
            InnerBounds = new Rectangle(0, 0, innerSizeX, innerSizeY);
            Position = Vector2.Zero;
            IsDraggable = true;

            kernel.RegisterWindow(this);
        }

        private void UpdatePosition()
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (Buttons.Count == 0)
            {
                _orderedButtons = Array.Empty<WindowButtonDrawInfo>();
                _maybeButtonsBounds = Rectangle.Empty;
            }
            _orderedButtons = Buttons.Values.OrderBy(b => b.Order).Select((b, i) =>
                new WindowButtonDrawInfo(b,
                    new Rectangle((OuterBounds.Location.ToVector2() + new Vector2(OuterBounds.Width - spacing - WindowButton.Size - (WindowButton.Size + spacing) * i, OuterBounds.Height - WindowButton.Size - spacing)).ToPoint(), new Point(WindowButton.Size))
                )
            ).ToArray();

            _maybeButtonsBounds = GetMaybeButtonBounds();
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

        private Rectangle _maybeButtonsBounds;

        private Rectangle GetMaybeButtonBounds()
        {
            int mbbWidth = _orderedButtons.Length * (WindowButton.Size + spacing) + spacing;
            int mbbHeight = (WindowButton.Size + spacing) + spacing;

            return new Rectangle(OuterBounds.X + OuterBounds.Width - mbbWidth, OuterBounds.Y + OuterBounds.Height - mbbHeight, mbbWidth, mbbHeight);
        }

        private bool MaybeHoveringButton()
        {
            return _maybeButtonsBounds.Contains(_input.Mouse.Position);
        }

        public void Lock()
        {
            if (BoundRegion is null) throw new InvalidOperationException("Can't lock unbound region.");

            BoundRegion.LockWindow();
        }

        public void Unlock()
        {
            if (BoundRegion is null) throw new InvalidOperationException("Can't unlock unbound region.");

            BoundRegion.UnlockWindow();
        }

        /// <summary>
        /// Brings this window to the front of the <see cref="Kernel"/> drawing order.
        /// Only calls <see cref="OnFocus()"/> if this window isn't focussed already.
        /// </summary>
        public void Focus()
        {
            if (_kernel.Focus(this)) // only run OnFocus if focussing changed anything
            {
                OnFocus();
            }
        }

        protected virtual void OnFocus() { }

        public Vector2 GetButtonPos(int index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Buttons.Count);
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            return OuterBounds.Location.ToVector2() + new Vector2(OuterBounds.Width - 2 - WindowButton.Size - (WindowButton.Size + 2) * index, OuterBounds.Height - WindowButton.Size - 2);
        }

        private Vector2? _dragOffset;

        public virtual void Update(int elapsedMilliseconds)
        {
            bool foundButton = false; // it should not be possible to click two buttons at once anyways.

            Dragging.Update(
                _input.IsMouseFocus(this) &&
                _input.IsClicked(MouseButton.Left) &&
                IsDraggable &&
                !IsLocked &&
                OuterBounds.Contains(_input.Mouse.Position) &&
                !MaybeHoveringButton(),
                _input, ref _dragOffset, this);

            if (_input.IsMouseFocus(this) && _input.IsClicked(MouseButton.Left) && OuterBounds.Contains(_input.Mouse.Position))
            {
                _kernel.Focus(this);
            }

            for (int i = 0; i < _orderedButtons.Length; i++)
            {
                WindowButtonDrawInfo buttonInfo = _orderedButtons[i];

                if (!foundButton && _input.IsMouseFocus(this) && buttonInfo.Bounds.Contains(_input.Mouse.Position))
                {
                    foundButton = true;

                    buttonInfo.Button.Hover(this);

                    if (_input.IsClicked(MouseButton.Left))
                        buttonInfo.Button.Click(this);
                }
                else
                {
                    buttonInfo.Button.Default(this);
                }
            }

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
                drawingContext.Draw(_orderedButtons[i].Button.Texture, GetButtonPos(i));
            }

            drawingContext.DrawString(_nameFont, Name,
                OuterBounds.Location.ToVector2() + new Vector2(3, (int)(OuterBounds.Height - 15 + _nameFont.Dimensions.Y / 2 - 3)));

            //if (_input.IsMouseFocus(this))
            //{
            //    drawingContext.DrawArea(OuterBounds, Color.Green);
            //}

            _input.RegisterDraw(this, OuterBounds);
        }

        protected abstract void DrawContents(DrawingContext drawingContext);
    }
}
