namespace STOLON
{
    public abstract class ShellRegion : IComponent
    {
        public abstract int Height { get; } // no buildin clearance svp

        protected Shell Shell { get; }

        protected Vector2 Position => Shell.GetRegionPos(this);

        public virtual int VerticalOverlap => 0;

        internal ShellRegion(Shell shell)
        {
            Shell = shell;
        }

        public virtual void Update(int elapsedMilliseconds) { }
        public virtual void Draw(DrawingContext drawingContext) { }
    }
}
