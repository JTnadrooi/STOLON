using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    public class Font2D // yeah i know 3d fonts are rare but they do exist (and I want the naming to be inline with Texture2D)
    {
        public string Id { get; }
        public SpriteFont SpriteFont { get; }
        public float Scale { get; }
        public Vector2 Dimensions { get; }
        public Font2D(string name, SpriteFont spriteFont, float scale = 1)
        {
            Id = name[(Font2DCollection.BASE_PATH.Length + 1)..];
            SpriteFont = spriteFont;
            Dimensions = spriteFont.MeasureString("A") * scale;
            Scale = scale;
        }
        public Vector2 FastMeasure(int i) => new Vector2(Dimensions.X * i + Math.Max(i * SpriteFont.Spacing, 0) * Scale, Dimensions.Y);
        public Vector2 FastMeasure(string s) => FastMeasure(s.Length);
        public override string ToString() => Id + " (Scale: " + Scale + ", Dimensions: " + Dimensions + ")";
        public static implicit operator SpriteFont(Font2D font) => font.SpriteFont;
    }
}
