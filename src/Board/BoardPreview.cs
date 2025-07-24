using AsitLib;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    public class BoardPreview : IGraphic
    {
        public BoardState SourceState { get; }
        public Vector2 Pos { get; }
        public BoardPreview(BoardState state)
        {
            SourceState = state;
        }
        public void Draw(DrawingContext drawingContext)
        {
            for (int x = 0; x < SourceState.Dimensions.X; x++)
                for (int y = 0; y < SourceState.Dimensions.Y; y++)
                {
                    drawingContext.Draw(STOLON.Textures["Debug\\temp-32"], Pos + new Vector2(x, y) * 32);
                }
        }
    }
}
