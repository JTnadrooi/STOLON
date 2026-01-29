using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public abstract class Window : IComponent
    {
        private readonly record struct WindowButtonDrawInfo(WindowButton Button, Rectangle Bounds);

        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;
        private readonly IFont2DCollection _fonts;
        private readonly IInputManager _input;

        public bool IsDraggable { get; protected set; }
        public bool IsResizable { get; protected set; }
        public bool IsBorderless { get; protected set; }

        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets whenever  <see cref="Update(int)"/> and <see cref="Draw(DrawingContext)"/> get called by the <see cref="Kernel"/>.
        /// </summary>
        public bool IsManaged { get; set; }

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

        protected Window(Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, int innerSizeX, int innerSizeY, string? name = null)
        {
            _textures = textures;
            _kernel = kernel;
            _fonts = fonts;
            _input = input;
            _nameFont = fonts.Medium;

            _buttons = new TypeDictionary<WindowButton>();
            _orderedButtons = Array.Empty<WindowButtonDrawInfo>();

            IsManaged = true;
            InnerBounds = new Rectangle(0, 0, innerSizeX, innerSizeY);
            Border = new Border(_textures["UI\\Window\\window-border"], 15, 1, 1, 1);
            Name = name ?? string.Empty;

            kernel.RegisterWindow(this);
        }

        private void UpdatePosition()
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            _orderedButtons = Buttons.Values.OrderBy(b => b.Order).Select((b, i) =>
                new WindowButtonDrawInfo(b,
                    new Rectangle((OuterBounds.Location.ToVector2() + new Vector2(OuterBounds.Width - 2 - WindowButton.Size - (WindowButton.Size + 2) * i, OuterBounds.Height - WindowButton.Size - 2)).ToPoint(), new Point(WindowButton.Size))
                )
            ).ToArray();
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

        public virtual void Update(int elapsedMilliseconds)
        {
            TransformMatrix = Matrix.CreateTranslation(InnerBounds.Location.X, InnerBounds.Location.Y, 0);

            bool foundButton = false; // it should not be possible to click two buttons at once anyways.

            for (int i = 0; i < _orderedButtons.Length; i++)
            {
                WindowButtonDrawInfo buttonInfo = _orderedButtons[i];

                if (!foundButton && buttonInfo.Bounds.Contains(_input.VirtualMousePos))
                {
                    foundButton = true;

                    buttonInfo.Button.Hover(this);

                    if (_input.IsClicked(MouseButton.Left)) buttonInfo.Button.Click(this);
                }
                else
                {
                    buttonInfo.Button.Default(this);
                }
            }

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
                drawingContext.Draw(_orderedButtons[i].Button.Texture,
                    OuterBounds.Location.ToVector2() + new Vector2(OuterBounds.Width - 2 - WindowButton.Size - (WindowButton.Size + 2) * i, OuterBounds.Height - WindowButton.Size - 2));
            }

            drawingContext.DrawString(_nameFont, Name,
                OuterBounds.Location.ToVector2() + new Vector2(3, (int)(OuterBounds.Height - 15 + _nameFont.Dimensions.Y / 2 - 3)));
        }

        protected abstract void DrawContents(DrawingContext drawingContext);
    }
}
