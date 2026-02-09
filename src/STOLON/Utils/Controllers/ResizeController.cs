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
        public Point? MaxSize { get; set; }
        public Point? MinSize { get; set; }

        private readonly IInputManager _input;
        private readonly Func<Sides?> _getResizeSides;

        private readonly Func<Rectangle> _getValue;
        private readonly Action<Rectangle> _setValue;

        private Vector2? _dragOrigin;
        private Sides? _dragSides;
        private Rectangle? _draginitialBounds;

        public ResizeController(
            IInputManager input,
            Func<Sides?> getResizeSides,
            Func<Rectangle> getValue,
            Action<Rectangle> setValue)
        {
            _input = input;
            _getResizeSides = getResizeSides;
            _getValue = getValue;
            _setValue = setValue;
        }

        public void Update(int elapsedMilliseconds)
        {
            Sides? mouseSides;

            if (_input.IsClicked(MouseButton.Left) && _dragSides is null && (mouseSides = _getResizeSides.Invoke()) is not null)
            {
                _dragOrigin = _input.Mouse.Position;
                _dragSides = mouseSides;
                _draginitialBounds = _getValue.Invoke();
            }

            if (!_input.IsPressed(MouseButton.Left))
            {
                _dragOrigin = null;
                _dragSides = null;
                _draginitialBounds = null;
            }

            if (_dragSides is not null)
            {
                Point delta = (_input.Mouse.Position - _dragOrigin!.Value).ToPoint();
                Rectangle newBounds = _draginitialBounds!.Value;

                if ((_dragSides.Value & Sides.Right) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y, delta.X + newBounds.Width, newBounds.Height);
                }
                if ((_dragSides.Value & Sides.Top) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y, newBounds.Width, delta.Y + newBounds.Height);
                }
                if ((_dragSides.Value & Sides.Left) != 0)
                {
                    newBounds = new Rectangle(newBounds.X + delta.X, newBounds.Y, newBounds.Width - delta.X, newBounds.Height);
                }
                if ((_dragSides.Value & Sides.Bottom) != 0)
                {
                    newBounds = new Rectangle(newBounds.X, newBounds.Y + delta.Y, newBounds.Width, newBounds.Height - delta.Y);
                }

                _setValue.Invoke(newBounds.ClampRectangle(_dragSides.Value, MinSize ?? Point.Zero, MaxSize ?? new Point(int.MaxValue)));
            }
        }
    }
}
