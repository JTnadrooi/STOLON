namespace STOLON
{
    public interface IOverlayManager
    {
        void Activate(string overlayId, params object?[] args);
        void AddOverlay(IOverlay overlay);
        void AddOverlay<TOverlay>() where TOverlay : IOverlay, new();
        void Deactivate(string overlayId);
        void Draw(DrawingContext drawingContext);
        bool IsActive(IOverlay overlay);
        bool IsActive(string overlayId);
        void RemoveOverlay(string overlayId);
        void Update(int elapsedMilliseconds);
    }
}