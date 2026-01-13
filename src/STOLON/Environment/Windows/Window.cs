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
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public bool Draggable { get; protected set; }
        public bool Resizable { get; protected set; }

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

        private Rectangle _innerBounds;

        protected Window(Kernel kernel, ITexture2DCollection textures, int innerSizeX, int innerSizeY)
        {
            _textures = textures;
            _kernel = kernel;

            InnerBounds = new Rectangle(0, 0, innerSizeX, innerSizeY);

            Border = new Border(_textures["UI\\Borders\\window-border"], 11, 4, 4, 4);

            kernel.RegisterWindow(this);
        }

        protected Vector2 ScreenToLocal(Point screenPosition) => ScreenToLocal(screenPosition.ToVector2());
        protected Vector2 ScreenToLocal(Vector2 screenPosition)
        {
            return screenPosition - InnerBounds.Location.ToVector2();
        }

        public virtual void Update(int elapsedMilliseconds)
        {
            TransformMatrix = Matrix.CreateTranslation(InnerBounds.Location.X, InnerBounds.Location.Y, 0);

            UpdateContents(elapsedMilliseconds);
        }

        protected virtual void UpdateContents(int elapsedMilliseconds) { }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.SetDrawingParameters(scissorArea: InnerBounds, transformMatrix: Matrix.CreateTranslation(InnerBounds.Location.X, InnerBounds.Location.Y, 0));
            drawingContext.SetDrawingParameters(scissorArea: InnerBounds, transformMatrix: TransformMatrix);

            DrawContents(drawingContext);

            drawingContext.SetDrawingParameters();

            drawingContext.DrawBorderAround(Border, InnerBounds);
        }

        protected abstract void DrawContents(DrawingContext drawingContext);
    }
}
