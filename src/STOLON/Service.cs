namespace STOLON
{
    /// <summary>
    /// A interface that provides a basic way to interact with <see cref="Intrara"/> component classes.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Update this component so it computes all the calculations.
        /// </summary>
        /// <param name="elapsedMilliseconds">The milliseconds since last frame.</param>
        public void Update(int elapsedMilliseconds) { }
        /// <summary>
        /// Update this component so it draws all sub-drawables.
        /// </summary>
        /// <param name="elapsedMilliseconds">The milliseconds since last frame.</param>
        public void Draw(DrawingContext drawingContext) { }
    }

    public abstract class Service : IService
    {
        public IService? Source { get; }

        protected Service(IService? source)
        {
            Source = source;
        }

        public virtual void Update(int elapsedMilliseconds)
        {

        }

        public virtual void Draw(DrawingContext drawingContext)
        {

        }
    }
}
