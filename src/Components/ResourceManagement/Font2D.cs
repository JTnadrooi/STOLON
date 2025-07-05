using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    /// <summary>
    /// Represents a monospace font.
    /// </summary>
    public class Font2D // yeah i know 3d fonts are rare but they do exist (and I want the naming to be inline with Texture2D)
    {
        public string Name { get; }
        public SpriteFont SpriteFont { get; }
        public float Scale { get; }
        public Vector2 Dimensions { get; }
        public const char BASE_CHAR = 'A';
        public Font2D(string name, SpriteFont spriteFont, float scale = 1)
        {
            Name = name;
            SpriteFont = spriteFont;
            Dimensions = spriteFont.MeasureString(BASE_CHAR.ToString()) * scale;
            Scale = scale;
        }
        public Vector2 FastMeasure(int i) => new Vector2(Dimensions.X * i + Math.Max(i * SpriteFont.Spacing, 0) * Scale, Dimensions.Y);
        public Vector2 FastMeasure(string s) => FastMeasure(s.Length);

        public string InBounds(string text, Rectangle bounds, int padding, out int lineCount) => InBounds(text, bounds.Width, bounds.Height, padding, out lineCount);
        public string InBounds(string text, int maxLineWidth, int maxLineHeight, int padding, out int lineCount)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            lineCount = 0;

            foreach (string word in words)
            {
                Vector2 size = FastMeasure(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + Dimensions.X;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineCount++;
                    lineWidth = size.X + Dimensions.X;
                }
            }

            return sb.ToString();
        }

        public override string ToString() => Name + " (Scale: " + Scale + ", Dimensions: " + Dimensions + ")";
        public static implicit operator SpriteFont(Font2D font) => font.SpriteFont;
    }
}
