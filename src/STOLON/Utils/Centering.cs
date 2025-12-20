namespace STOLON
{
    public static class Centering
    {
        //public static Vector2 TopLeft(Texture2D texture, Vector2 location, Vector2 scaling) => location;
        //public static Vector2 TopRight(Texture2D texture, Vector2 location, Vector2 scaling) => location + new Vector2(-(texture.Width * scaling.X), 0);
        //public static Vector2 BottomLeft(Texture2D texture, Vector2 location, Vector2 scaling) => location + new Vector2(0, -(texture.Height * scaling.Y));
        //public static Vector2 BottomRight(Texture2D texture, Vector2 location, Vector2 scaling) => location + new Vector2(-(texture.Width * scaling.X), -(texture.Height * scaling.Y));

        public static Vector2 TopLeft(Texture2D texture, Vector2 pos, Vector2 scaling) => pos + new Vector2(0, texture.Height * scaling.Y);
        public static Vector2 TopRight(Texture2D texture, Vector2 pos, Vector2 scaling) => pos + new Vector2(texture.Width * scaling.X, texture.Height * scaling.Y);

        public static Vector2 BottomLeft(Texture2D texture, Vector2 pos, Vector2 scaling) => pos;
        public static Vector2 BottomRight(Texture2D texture, Vector2 pos, Vector2 scaling) => pos + new Vector2(texture.Width * scaling.X, 0);

        public static Vector2 CenterX(Texture2D texture, float yPosition, float containerWidth, Vector2 scale) => CenterX(texture, yPosition, containerWidth, scale.X);
        public static Vector2 CenterX(int contentWidth, float yPosition, float containerWidth, Vector2 scale) => CenterX(contentWidth, yPosition, containerWidth, scale.X);
        public static Vector2 CenterX(Texture2D texture, float yPosition, float containerWidth, float scale = 1f) => CenterX(texture.Width, yPosition, containerWidth, scale);
        public static Vector2 CenterX(int contentWidth, float yPosition, float containerWidth, float scale = 1f) => new Vector2(containerWidth * 0.5f - contentWidth * scale * 0.5f, yPosition);

        public static Vector2 CenterY(Texture2D texture, float xPosition, float containerHeight, Vector2 scale) => CenterY(texture.Height, xPosition, containerHeight, scale.Y);
        public static Vector2 CenterY(int contentHeight, float xPosition, float containerHeight, Vector2 scale) => CenterY(contentHeight, xPosition, containerHeight, scale.Y);
        public static Vector2 CenterY(Texture2D texture, float xPosition, float containerHeight, float scale = 1f) => CenterY(texture.Height, xPosition, containerHeight, scale);
        public static Vector2 CenterY(int contentHeight, float xPosition, float containerHeight, float scale = 1f) => new Vector2(xPosition, containerHeight * 0.5f - contentHeight * scale * 0.5f);

        public static Vector2 Center(Rectangle innerRect, Rectangle outerRect)
        {
            float x = outerRect.X + (outerRect.Width - innerRect.Width) * 0.5f;
            float y = outerRect.Y + (outerRect.Height - innerRect.Height) * 0.5f;
            return new Vector2(x, y);
        }

        public static Vector2 Center(Texture2D texture, Rectangle containerRect, Vector2 scale) => Center((new Vector2(texture.Width, texture.Height) * scale).ToPoint(), containerRect);

        public static Vector2 Center(Point contentSize, Rectangle containerRect) => Center(new Rectangle(Point.Zero, contentSize), containerRect);

        public static Vector2 Get(Rectangle rectangle) => rectangle.Center.ToVector2();
        public static Vector2 Get(Texture2D texture) => Get(texture.Bounds);

        public static void OnPixel(ref Vector2 pos) => pos = pos.ToPoint().ToVector2();

        //public static void DrawStringCenterX(this DrawingContext context, Font2D font, string text, Rectangle bounds, Vector2 positionOffset, float scale = 1f, float rotation = 0f, Vector2? origin = null, Color? color = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        //    => context.DrawString(font, text, bounds.Location.ToVector2() + Centering.CenterX((int)STOLON.Fonts.Medium.FastMeasure(text).X, 0, bounds.Width, scale), new Vector2(scale), rotation, origin, color, effects, layerDepth);
    }

    public enum Orgin
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }
}
