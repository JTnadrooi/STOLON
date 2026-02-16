namespace STOLON
{
    public interface IOverlay : IComponent
    {
        public void Initialize(OverlayManager overlayer, params object?[] args);
        public void Reset();

        public string Id { get; }
        public bool IsFinished { get; }
    }
}
