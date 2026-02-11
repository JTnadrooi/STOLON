namespace STOLON
{
    public abstract class ShellRegion : IComponent
    {
        public abstract int Height { get; } // no buildin clearance svp
        public abstract int Width { get; }

        protected Shell Shell { get; }

        protected Vector2 Position => Shell.GetRegionPos(this);

        public virtual int VerticalOverlap => 0;

        internal ShellRegion(Shell shell)
        {
            Shell = shell;
        }

        public Rectangle Bounds => new Rectangle(((int)Position.X), ((int)Position.Y), Width, Height);

        public virtual void Update(int elapsedMilliseconds) { }
        public virtual void Draw(DrawingContext drawingContext) { }
    }
}
