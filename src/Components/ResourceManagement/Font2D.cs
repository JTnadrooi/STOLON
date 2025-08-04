using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using System;
using System.Linq;
using System.Text;

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
            Scale = scale;
            Dimensions = new Vector2(CoreFont.GetGlyphs(BASE_CHAR.ToString()).First().Character.XAdvance * scale, CoreFont.LineHeight * scale);
        }
        public Vector2 FastMeasure(int i) => new Vector2(Dimensions.X * i, Dimensions.Y);
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

    public static class Font2DDrawingExtensions
    {
        public static void DrawString(this DrawingContext context, Font2D font, string text, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => context.DrawString(font, text, position, new Vector2(scale), rotation, origin, color, effects, layerDepth);
        public static void DrawString(this DrawingContext context, Font2D font, string text, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            int GetUnicodeCodePoint(string text, ref int index) => (!char.IsHighSurrogate(text[index]) || ++index >= text.Length) ? text[index] : char.ConvertToUtf32(text[index - 1], text[index]);
            unsafe int CountNewline(string input)
            {
                int count = 0;
                fixed (char* ptr = input)
                {
                    char* current = ptr;
                    char* end = ptr + input.Length;
                    while (current < end)
                    {
                        if (*current == '\n') count++;
                        current++;
                    }
                }
                return count;
            }

            if (text == null) throw new ArgumentNullException("text");

            BitmapFont.BitmapFontGlyph currentGlyph;
            BitmapFont.BitmapFontGlyph? previousGlyph = null;
            Vector2 positionDelta = new Vector2(0, CountNewline(text) * font.Dimensions.Y * scale.Y);

            for (int i = 0; i < text.Length; i++)
            {
                int unicodeCodePoint = GetUnicodeCodePoint(text, ref i);
                currentGlyph.CharacterID = unicodeCodePoint;
                if (!font.CoreFont.TryGetCharacter(unicodeCodePoint, out currentGlyph.Character))
                    throw new ArgumentNullException("unsupported char: " + unicodeCodePoint);

                currentGlyph.Position = position + positionDelta;

                currentGlyph.Position.X += currentGlyph.Character.XOffset;
                currentGlyph.Position.Y += currentGlyph.Character.YOffset;
                positionDelta.X += currentGlyph.Character.XAdvance + font.CoreFont.LetterSpacing;

                previousGlyph = currentGlyph;
                if (unicodeCodePoint == 10) // newline.
                {
                    positionDelta.Y -= font.CoreFont.LineHeight;
                    positionDelta.X = 0f;
                    previousGlyph = null;
                }

                context.SpriteBatch.Draw(currentGlyph.Character.TextureRegion, position, color ?? Color.White, rotation, position - currentGlyph.Position + (origin ?? Vector2.Zero), scale, context.InvertY(effects), layerDepth);
            }
        }
    }
}
