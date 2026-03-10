namespace STOLON
{
    public abstract class ShellRegion : IComponent
    {
        public abstract int Height { get; } // no built-in clearance svp, should be FAST.
        public abstract int Width { get; }

        protected Shell Shell { get; }

        private Vector2 _position;
        private int _lastHeightForPosition;

        public Vector2 Position
        {
            get
            {
                if (Height != _lastHeightForPosition)
                {
                    Shell.UpdateRegionPositions(false);
                }
                return _position;
            }

            internal set
            {
                _lastHeightForPosition = Height;
                _position = value;
            }
        }

        public virtual int VerticalOverlap => 0;

        internal ShellRegion(Shell shell)
        {
            Shell = shell;
        }

        public Rectangle Bounds => new Rectangle(((int)Position.X), ((int)Position.Y), Width, Height);

        public virtual void Update(int elapsedMilliseconds) { }
        public virtual void Draw(DrawingContext drawingContext) { }

        /// <summary>
        /// Called after all regions have been drawn. Override this method to draw elements that should overlap with other regions.
        /// </summary>
        public virtual void PostDraw(DrawingContext drawingContext) { }
    }
}
