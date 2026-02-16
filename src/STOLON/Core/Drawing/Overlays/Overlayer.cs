using System.Numerics;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public class OverlayManager : IComponent, IOverlayManager
    {
        private Dictionary<string, IOverlay> _overlayDict;
        private List<string> _initialized;

        private readonly IRichLogger _logger;
        private readonly IEnumerable<IOverlay> _overlays;

        public OverlayManager(IRichLogger logger, IEnumerable<IOverlay> overlays)
        {
            _logger = logger;
            _overlays = overlays;

            _overlayDict = new Dictionary<string, IOverlay>();
            _initialized = new List<string>();

            _logger.Log(">searching for overlays");
            foreach (IOverlay overlay in overlays)
            {
                _logger.Log($"found overlay with id '{overlay.Id}\".");
                AddOverlay(overlay);
            }
            _logger.Success();
        }

        public void AddOverlay<TOverlay>() where TOverlay : IOverlay, new() => AddOverlay(new TOverlay());
        public void AddOverlay(IOverlay overlay)
        {
            _logger.Log(">adding overlay of id " + overlay.Id + ".");
            _overlayDict.Add(overlay.Id, overlay);
            _logger.Success();
        }

        public void RemoveOverlay(string overlayId)
        {
            _logger.Log(">removing overlay of id " + overlayId + ".");
            Deactivate(overlayId);
            _overlayDict.Remove(overlayId);
            _logger.Success();
        }

        public void Activate(string overlayId, params object?[] args)
        {

            if (!_initialized.Contains(overlayId))
            {
                _logger.Log(">[s]activating overlay of id " + overlayId + ".");
                _overlayDict[overlayId].Initialize(this, args);
                _initialized.Add(overlayId);
                _logger.Success();
            }
        }

        public bool IsActive(IOverlay overlay) => IsActive(overlay.Id);
        public bool IsActive(string overlayId)
        {
            return _initialized.Contains(overlayId);
        }

        public void Deactivate(string overlayId) // ensure
        {
            if (_initialized.Contains(overlayId))
            {
                _logger.Log(">deactivating overlay of id " + overlayId + ".");
                _overlayDict[overlayId].Reset();
                _logger.Success();
            }
            _initialized.Remove(overlayId);
        }

        public void Update(int elapsedMilliseconds)
        {
            IOverlay overlay;
            for (int i = 0; i < _initialized.Count; i++) // for all initialized overlays
            {
                overlay = _overlayDict[_initialized[i]];
                overlay.Update(elapsedMilliseconds);
                if (overlay.IsFinished)
                {
                    _logger.Log(">deactivating and resetting ended overlay of id " + overlay.Id + ".");
                    Deactivate(overlay.Id);
                    _logger.Success();
                }
            }
        }
        public void Draw(DrawingContext drawingContext)
        {
            for (int i = 0; i < _initialized.Count; i++)
            {
                _overlayDict[_initialized[i]].Draw(drawingContext);
            }
        }
    }
}
