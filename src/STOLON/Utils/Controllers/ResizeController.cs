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

        private readonly Func<Sides> _getResizeSides;
        private readonly Func<Rectangle> _getValue;
        private readonly Action<Rectangle> _setValue;
        private readonly Action? _onResizeStart;

        private Vector2? _dragOrigin;
        private Sides _dragSides;
        private Rectangle? _draginitialBounds;

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

            if (_input.IsClicked(MouseButton.Left) && _dragSides == Sides.None && (mouseSides = _getResizeSides.Invoke()) != Sides.None)
            {
                _dragOrigin = _input.Mouse.Position;
                _dragSides = mouseSides;
                _draginitialBounds = _getValue.Invoke();
                _onResizeStart?.Invoke();
            }

            if (!_input.IsPressed(MouseButton.Left))
            {
                _dragOrigin = null;
                _dragSides = Sides.None;
                _draginitialBounds = null;
            }

            if (_dragSides != Sides.None)
            {
                Point delta = (_input.Mouse.Position - _dragOrigin!.Value).ToPoint();
                Rectangle newBounds = _draginitialBounds!.Value;

                if ((_dragSides & Sides.Right) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y, delta.X + newBounds.Width, newBounds.Height);
                }
                if ((_dragSides & Sides.Top) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y, newBounds.Width, delta.Y + newBounds.Height);
                }
                if ((_dragSides & Sides.Left) != 0)
                {
                    newBounds = new Rectangle(newBounds.X + delta.X, newBounds.Y, newBounds.Width - delta.X, newBounds.Height);
                }
                if ((_dragSides & Sides.Bottom) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y + delta.Y, newBounds.Width, newBounds.Height - delta.Y);
                }

                _setValue.Invoke(newBounds.ClampRectangle(_dragSides, MinSize ?? Point.Zero, MaxSize ?? new Point(int.MaxValue)));
            }
        }
    }
}
