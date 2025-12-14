using AsitLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static STOLON.UIElement;

namespace STOLON
{
    public interface IGraphic
    {
        void Draw(DrawingContext drawingContext);
    }
}
