using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    public class GameFont
    {
        public string Name { get; }
        public SpriteFont SpriteFont { get; }
        public float Scale { get; }
        public Vector2 Dimensions { get; }
        public GameFont(string name, SpriteFont spriteFont, float scale = 1)
        {
            Name = name;
            SpriteFont = spriteFont;
            Dimensions = spriteFont.MeasureString("A") * scale;
            Scale = scale;
        }
        public Vector2 FastMeasure(int i) => new Vector2(Dimensions.X * i + Math.Max(i * SpriteFont.Spacing, 0) * Scale, Dimensions.Y);
        public Vector2 FastMeasure(string s) => FastMeasure(s.Length);
        public override string ToString() => Name + " (Scale: " + Scale + ", Dimensions: " + Dimensions + ")";
        public static implicit operator SpriteFont(GameFont font) => font.SpriteFont;
    }
}
