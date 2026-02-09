using STOLON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class DragController : IController
    {
        private readonly IInputManager _input;
        private readonly Func<bool> _shouldInitiateDrag;

        private readonly Func<Vector2> _getValue;
        private readonly Action<Vector2> _setValue;
        private readonly Action? _onDragStart;

        private Vector2? _initialDragOffset;

        public DragController(
            IInputManager input,
            Func<bool> shouldInitiateDrag,
            Func<Vector2> getValue,
            Action<Vector2> setValue,
            Action? onDragStart = null)
        {
            _input = input;
            _shouldInitiateDrag = shouldInitiateDrag;
            _getValue = getValue;
            _setValue = setValue;
            _onDragStart = onDragStart;
        }

        public void Update(int elapsedMilliseconds)
        {
            if (_shouldInitiateDrag.Invoke())
            {
                _initialDragOffset = _getValue.Invoke() - _input.Mouse.Position;
                _onDragStart?.Invoke();
            }

            if (!_input.IsPressed(MouseButton.Left))
            {
                _initialDragOffset = null;
            }

            if (_initialDragOffset is not null)
            {
                _setValue.Invoke(_input.Mouse.Position + _initialDragOffset.Value);
            }
        }
    }
}
