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
        public BitmapFont CoreFont { get; }
        public float Scale { get; }
        public Vector2 Dimensions { get; }
        public const char BASE_CHAR = 'A';
        public Font2D(string name, BitmapFont spriteFont, float scale = 1)
        {
            Name = name;
            CoreFont = spriteFont;
            Dimensions = spriteFont.MeasureString(BASE_CHAR.ToString()) * scale;
            Scale = scale;
        }
        public Vector2 FastMeasure(int i) => new Vector2(Dimensions.X * i + Math.Max(i * CoreFont.LetterSpacing - 1, 0) * Scale - 1, Dimensions.Y);
        public Vector2 FastMeasure(string s) => FastMeasure(s.Length);

        public string Wrap(string text, Rectangle bounds, int padding, out int lineCount) => Wrap(text, bounds.Width, bounds.Height, padding, out lineCount);
        public string Wrap(string text, int maxLineWidth, int maxLineHeight, int padding, out int lineCount)
        {
            if (string.IsNullOrEmpty(text))
            {
                lineCount = 0;
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length + 32);
            var words = text.AsSpan();
            int start = 0;
            float lineW = 0;
            int lines = 1;

            int maxLines = (int)((maxLineHeight - 2 * padding) / Dimensions.Y);
            if (maxLines <= 0) { lineCount = 0; return string.Empty; }

            while (start < words.Length)
            {
                int space = words[start..].IndexOf(' ');
                bool last = space == -1;

                ReadOnlySpan<char> span = last ? words[start..] : words.Slice(start, space);
                string word = span.ToString();
                float wordW = FastMeasure(word).X + Dimensions.X;

                // will the word fit on this line?
                if (lineW + wordW > maxLineWidth)
                {
                    // would a new line fit vertically?
                    if (lines + 1 > maxLines)
                    {
                        sb.Append("...");
                        break; // no more space
                    }

                    sb.Append('\n');
                    lineW = 0;
                    lines++;
                }

                if (lineW > 0) sb.Append(' ');
                sb.Append(word);
                lineW += wordW;

                if (!last) start += space + 1;
                else break;
            }

            lineCount = lines;
            return sb.ToString();
        }

        public override string ToString() => Name + " (Scale: " + Scale + ", Dimensions: " + Dimensions + ")";
        public static implicit operator BitmapFont(Font2D font) => font.CoreFont;
    }
}
