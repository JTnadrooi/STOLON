namespace STOLON
{
    public abstract class ShellRegion
    {
        public abstract int Height { get; } // no buildin clearance svp

        protected Shell Shell { get; }

        protected Vector2 Pos => Shell.GetRegionPos(this);

        //protected bool HasMouse =>

        public virtual int VerticalOverlap => 0;

        public ShellRegion(Shell shell)
        {
            Shell = shell;
        }

        public virtual void Update(int elapsedMilliseconds) { }
        public virtual void Draw(DrawingContext drawingContext) { }
    }
}
