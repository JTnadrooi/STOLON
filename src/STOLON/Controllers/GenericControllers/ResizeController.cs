using STOLON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class ResizeController : IController
    {
        private readonly IInputManager _input;

        public Point? MaxSize { get; set; }
        public Point? MinSize { get; set; }

        public bool IsResizing => _resizeOrigin.HasValue;

        private readonly Func<Sides> _getResizeSides;
        private readonly Func<Rectangle> _getValue;
        private readonly Action<Rectangle> _setValue;
        private readonly Action? _onResizeStart;

        private Vector2? _resizeOrigin;
        private Sides _resizingSides;
        private Rectangle? _initialBounds;

        public ResizeController(
            IInputManager input,
            Func<Sides> getResizeSides,
            Func<Rectangle> getValue,
            Action<Rectangle> setValue,
            Action? onResizeStart = null)
        {
            _input = input;

            _getResizeSides = getResizeSides;
            _getValue = getValue;
            _setValue = setValue;
            _onResizeStart = onResizeStart;
        }

        public void Update(int elapsedMilliseconds)
        {
            Sides mouseSides;

            if (_input.IsClicked(MouseButton.Left) && _resizingSides == Sides.None && (mouseSides = _getResizeSides.Invoke()) != Sides.None)
            {
                _resizeOrigin = _input.Mouse.Position;
                _resizingSides = mouseSides;
                _initialBounds = _getValue.Invoke();
                _onResizeStart?.Invoke();
            }

            if (!_input.IsPressed(MouseButton.Left))
            {
                _resizeOrigin = null;
                _resizingSides = Sides.None;
                _initialBounds = null;
            }

            if (_resizingSides != Sides.None)
            {
                Point delta = (_input.Mouse.Position - _resizeOrigin!.Value).ToPoint();
                Rectangle newBounds = _initialBounds!.Value;

                if ((_resizingSides & Sides.Right) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y, delta.X + newBounds.Width, newBounds.Height);
                }
                if ((_resizingSides & Sides.Top) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y, newBounds.Width, delta.Y + newBounds.Height);
                }
                if ((_resizingSides & Sides.Left) != 0)
                {
                    newBounds = new Rectangle(newBounds.X + delta.X, newBounds.Y, newBounds.Width - delta.X, newBounds.Height);
                }
                if ((_resizingSides & Sides.Bottom) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y + delta.Y, newBounds.Width, newBounds.Height - delta.Y);
                }

                _setValue.Invoke(newBounds.ClampRectangle(_resizingSides, MinSize ?? Point.Zero, MaxSize ?? new Point(int.MaxValue)));
            }
        }
    }
}
