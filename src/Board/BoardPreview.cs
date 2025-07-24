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
        public Vector2 Pos { get; set; }
        public Point Dimensions => SourceState.Dimensions;

        private Texture2D _tileTexure;

        public const int TILE_SIZE = 16;

        public BoardPreview(BoardState state)
        {
            SourceState = state;

            _tileTexure = STOLON.Textures["Debug\\temp-" + TILE_SIZE];
        }
        public void Draw(DrawingContext drawingContext)
        {
            for (int x = 0; x < SourceState.Dimensions.X; x++)
                for (int y = 0; y < SourceState.Dimensions.Y; y++)
                {
                    drawingContext.Draw(_tileTexure, Pos + new Vector2(x, y) * TILE_SIZE);
                }
        }
    }
}
