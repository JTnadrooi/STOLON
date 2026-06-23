using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public static class DebugDrawing
    {
        private static Queue<Action<DrawingContext>> queuedDrawActions = new Queue<Action<DrawingContext>>(128);

        public static void EnqueueDraw(Action<DrawingContext> drawAction)
        {
            queuedDrawActions.Enqueue(drawAction);
        }

        public static void DrawAll(DrawingContext drawingContext)
        {
            while (queuedDrawActions.TryDequeue(out Action<DrawingContext>? drawAction))
            {
                drawAction.Invoke(drawingContext);
            }
        }
    }
}
