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

        public bool IsDragging => _initialOffset.HasValue;

        private readonly Func<bool> _shouldInitiate;
        private readonly Func<Vector2> _getValue;
        private readonly Action<Vector2> _setValue;
        private readonly Action? _onDragStart;

        private Vector2? _initialOffset;

        public DragController(
            IInputManager input,
            Func<bool> shouldInitiate,
            Func<Vector2> getValue,
            Action<Vector2> setValue,
            Action? onDragStart = null)
        {
            _input = input;
            _shouldInitiate = shouldInitiate;
            _getValue = getValue;
            _setValue = setValue;
            _onDragStart = onDragStart;
        }

        public void Update(int elapsedMilliseconds)
        {
            if (_shouldInitiate.Invoke())
            {
                _initialOffset = _getValue.Invoke() - _input.Mouse.Position;
                _onDragStart?.Invoke();
            }

            if (!_input.IsPressed(MouseButton.Left))
            {
                _initialOffset = null;
            }

            if (_initialOffset is not null)
            {
                _setValue.Invoke(_input.Mouse.Position + _initialOffset.Value);
            }
        }
    }
}
