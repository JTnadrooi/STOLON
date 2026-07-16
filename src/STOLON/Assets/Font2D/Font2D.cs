using MonoGame.Extended.BitmapFonts;
using System.Runtime.CompilerServices;

namespace STOLON
{
    /// <summary>
    /// Represents a monospace font.
    /// </summary>
    public class Font2D // yeah i know 3d fonts are rare but they do exist (and I want the naming to be inline with Texture2D)
    {
        public BitmapFont CoreFont { get; }
        public float Scale { get; }
        public Vector2 Dimensions { get; }
        public const char BASE_CHAR = 'A';
        public Font2D(BitmapFont spriteFont, float scale = 1)
        {
            CoreFont = spriteFont;
            Scale = scale;
            Dimensions = new Vector2(CoreFont.GetGlyphs(BASE_CHAR.ToString()).First().Character.XAdvance * scale, CoreFont.LineHeight * scale);
        }
        public Vector2 FastMeasure(int i) => new Vector2(Dimensions.X * i, Dimensions.Y);
        public Vector2 FastMeasure(string s) => FastMeasure(s.Length);

        public string Wrap(string text, Rectangle bounds, out int lineCount) => Wrap(text, bounds.Width, bounds.Height, out lineCount);
        public string Wrap(string text, int maxLineWidth, int maxLineHeight, out int lineCount)
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

            int maxLines = (int)((maxLineHeight) / Dimensions.Y);
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

        public override string ToString() => $"{{Scale: {Scale}, Dimensions: {Dimensions}}}";
        public static implicit operator BitmapFont(Font2D font) => font.CoreFont;
        public static implicit operator Font2D(BitmapFont font) => new Font2D(font);
    }

    public static class Font2DDrawingExtensions
    {
        public static void DrawString(this DrawingContext context, Font2D font, string text, Vector2 position, float scale = 1f, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
            => context.DrawString(font, text, position, new Vector2(scale), rotation, origin, color, effects, layerDepth);
        public static void DrawString(this DrawingContext context, Font2D font, string text, Vector2 position, Vector2 scale, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            // WANRING !! this method breaks if you input a string with a newline before a space. if you're fixing this now then hello future nadrooi! 
            // edit: this may be caused by the shell/textregion classes, not by this

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int GetUnicodeCodePoint(string text, ref int index) => (!char.IsHighSurrogate(text[index]) || ++index >= text.Length) ? text[index] : char.ConvertToUtf32(text[index - 1], text[index]);

            ArgumentNullException.ThrowIfNull(text);

            float scaledLineHeight = font.CoreFont.LineHeight * scale.Y;
            float scaledLetterSpacing = font.CoreFont.LetterSpacing * scale.X;

            Vector2 positionDelta = new Vector2(0, ((ReadOnlySpan<char>)text).FastCount('\n') * scaledLineHeight + scaledLineHeight);

            for (int i = 0; i < text.Length; i++)
            {
                int unicodeCodePoint = GetUnicodeCodePoint(text, ref i);

                // carriage return; just reset horizontal position
                if (unicodeCodePoint == '\r')
                {
                    positionDelta.X = 0f;
                    continue;
                }

                // newline; move to next line and reset horizontal
                if (unicodeCodePoint == '\n')
                {
                    positionDelta.Y -= scaledLineHeight;
                    positionDelta.X = 0f;
                    continue;
                }

                // fetch glyph
                if (!font.CoreFont.TryGetCharacter(unicodeCodePoint, out var character))
                    throw new InvalidOperationException($"Unsupported codepoint '{unicodeCodePoint}'.");

                // compute glyph position with scaled offsets
                Vector2 glyphPos = position + positionDelta;
                glyphPos.X += character.XOffset * scale.X;
                glyphPos.Y -= (character.YOffset + character.TextureRegion.Size.Height) * scale.Y;

                // draw the glyph
                context.SpriteBatch.Draw(
                    character.TextureRegion,
                    glyphPos,
                    color ?? Color.White,
                    rotation,
                    origin ?? Vector2.Zero,
                    scale,
                    context.InvertY(effects),
                    layerDepth
                );

                // advance horizontal position for next glyph
                positionDelta.X += (character.XAdvance + font.CoreFont.LetterSpacing) * scale.X;
            }
        }
    }
}
