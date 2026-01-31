using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public static class Dragging
    {
        public static void Update(bool canInitiateDrag, IInputManager input, ref Vector2? dragOffset, IPositionable positionable)
        {
            if (input.IsClicked(MouseButton.Left))
            {
                if (canInitiateDrag)
                {
                    dragOffset = positionable.Position - input.VirtualMousePos;
                }
                else dragOffset = null;
            }

            if (!input.IsPressed(MouseButton.Left))
            {
                dragOffset = null;
            }

            if (dragOffset is not null)
            {
                positionable.Position = input.VirtualMousePos + dragOffset.Value;
            }
        }
    }
}
