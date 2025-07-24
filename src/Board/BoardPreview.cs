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
        public Rectangle Bounds { get; private set; }
        public Vector2 Pos
        {
            get => _pos;
            set {
                _pos = value;
                Bounds = new Rectangle(value.ToPoint(), Dimensions);
            }
        }
        public Point Dimensions => new Point(SourceState.Dimensions.X * TILE_SIZE, SourceState.Dimensions.Y * TILE_SIZE);

        private Texture2D _tileTexure;
        private Vector2 _pos;

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
                    //drawingContext.Draw(_tileTexure, Pos + new Vector2(x, y) * TILE_SIZE);
                    drawingContext.DrawString(STOLON.Fonts.Small, "?", Pos + new Vector2(x, y) * TILE_SIZE + new Vector2(TILE_SIZE / 2, 4));
                }
            drawingContext.DrawRectangle(Bounds, Color.White, 2);
        }
    }
}
