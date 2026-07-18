namespace STOLON
{
    /// <summary>
    /// What type the <see cref="UIElement"/> is. When <see cref="Listen"/>, "collisions" with the mouse will be calculated for its hitbox.
    /// </summary>
    public enum UIElementType
    {
        /// <summary>
        /// Its relevant when this <see cref="UIElement"/> gets clicked.
        /// </summary>
        Listen,

        /// <summary>
        /// Its not relevant when this <see cref="UIElement"/> gets clicked.
        /// </summary>
        Ignore,
    }

    /// <summary>
    /// Reprecents a button or textplane in the UI. Add new elements to the <see cref="UIPath"/> using the <see cref="UIPath.AddElement(UIElement)"/> method.
    /// </summary>
    public abstract class UIElement
    {
        public bool IsTop => ParentId == TopId;

        /// <summary>
        /// The type of the <see cref="UIElement"/>.
        /// </summary>
        public UIElementType Type { get; }

        public bool Skip { get; set; }

        /// <summary>
        /// The ID of the <see cref="UIElement"/>.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The order of this <see cref="UIElement"/>. From top to bottom. Yet to be implemented.
        /// </summary>
        public string? Order { get; }

        /// <summary>
        /// The <see cref="UIElement.Id"/> of the <see cref="UIElement"/> this is a child of. 
        /// </summary>
        public string ParentId { get; }

        public const string TopId = "_";
        public const string BackPrefix = "_back_";

        public UIElement(string id, string parentId = UIElement.TopId, UIElementType type = UIElementType.Listen, string? order = null, CachedAudio? clickSound = null)
        {
            Type = type;
            Id = id;
            Order = order;
            ParentId = parentId;
            Skip = false;
        }

        public abstract Rectangle GetBounds(Point pos, int paddingX, int paddingY, int marginX, int marginY);
    }

    public class TextElement : UIElement
    {
        public string Text { get; set; }
        public Font2D Font { get; set; }

        public TextElement(string id, string text, Font2D font, string parentId = "_", UIElementType type = UIElementType.Listen, string? order = null, CachedAudio? clickSound = null) : base(id, parentId, type, order, clickSound)
        {
            Text = text;
            Font = font;
        }

        public override Rectangle GetBounds(Point pos, int paddingX, int paddingY, int marginX, int marginY)
        {
            Vector2 contentSize = Font.FastMeasure(Text);
            int recW = (int)contentSize.X + 2 * paddingX;
            int recH = (int)contentSize.Y + 2 * paddingY;

            return new Rectangle(pos + new Point(marginX, marginY), new Point(recW, recH));
        }

        public Point GetTextPos(Rectangle preCalculatedBounds, int paddingX, int paddingY)
        {
            return new Point(preCalculatedBounds.X + paddingX, preCalculatedBounds.Y + paddingY);
        }

        public static TextElement GetDefaultBackElement(Font2D font, string parentId)
        {
            return new TextElement(BackPrefix + parentId, "Back", font, parentId, UIElementType.Listen);
        }
    }

    public class TextureElement : UIElement
    {
        public Texture2D Texture { get; set; }

        public TextureElement(string id, Texture2D texture, string parentId = "_", UIElementType type = UIElementType.Listen, string? order = null) : base(id, parentId, type, order)
        {
            Texture = texture;
        }

        public override Rectangle GetBounds(Point pos, int paddingX, int paddingY, int marginX, int marginY)
        {
            return new Rectangle(pos + new Point(marginX, marginY), Texture.Bounds.Size + new Point(2 * paddingX, 2 * paddingY));
        }
    }


    /// <summary>
    /// The data element relevant for draw methods.
    /// </summary>
    public class UIElementDrawData : IDrawable
    {
        /// <summary>
        /// The position of the "drawdataified" <see cref="UIElement"/>.
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// If a bounding <see cref="RectangleF"/> must be drawn.
        /// </summary>
        public bool DrawRectangle { get; }

        /// <summary>
        /// The bounding rectangle to draw.
        /// </summary>
        public Rectangle Rectangle { get; }

        /// <summary>
        /// The type of the <see cref="UIElement"/>. Sometimes relevant for drawing.
        /// </summary>
        public UIElementType Type { get; }

        /// <summary>
        /// The text to draw inside the <see cref="Rectangle"/>.
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// The texture to draw inside the <see cref="Rectangle"/>.
        /// </summary>
        public Texture2D? Texture { get; }
        public UIElement? Source { get; }
        public bool IsEmpty => Source == null;
        public bool Hide { get; }
        public bool DrawBackground { get; }
        public Font2D? Font { get; }

        /// <summary>
        /// Create a new <see cref="UIElementDrawData"/> object.
        /// </summary>
        public UIElementDrawData(UIElement? source, Texture2D texture, UIElementType type, Vector2 position, Rectangle rectangle, bool drawRectangle, bool hide = false, bool drawBg = false)
        {
            Position = position;
            Type = type;
            Texture = texture;
            Rectangle = rectangle;
            DrawRectangle = drawRectangle;
            Source = source;
            Hide = hide;
            DrawBackground = drawBg;
        }

        /// <summary>
        /// Create a new <see cref="UIElementDrawData"/> object.
        /// </summary>
        public UIElementDrawData(UIElement? source, string text, Font2D font, UIElementType type, Vector2 position, Rectangle rectangle, bool drawRectangle, bool hide = false, bool drawBg = false)
        {
            Position = position;
            Type = type;
            Text = text;
            Rectangle = rectangle;
            DrawRectangle = drawRectangle;
            Source = source;
            Font = font;
            Hide = hide;
            DrawBackground = drawBg;
        }

        public override string ToString()
        {
            if (Text is not null) return $"{{Id: '{Source}', Text: '{Text}', Type: {Type}, Position: {Position}, Rectangle: {Rectangle}, DrawRectangle: {DrawRectangle}, Draw: {Hide}, Font: {Font?.ToString() ?? "null"}}}";
            else return $"{{Id: '{Source}', Texture: '{Texture.Name}', Type: {Type}, Position: {Position}, Rectangle: {Rectangle}, DrawRectangle: {DrawRectangle}, Draw: {Hide}, Font: {Font?.ToString() ?? "null"}}}";
        }

        public void Draw(DrawingContext drawingContext)
        {
            if (Source is null) throw new InvalidOperationException("Cannot draw sourceless UI-element drawdata.");
            if (Hide) return;
            if (DrawBackground) drawingContext.DrawArea(Rectangle, Color.Black);
            if (Text is not null) drawingContext.DrawString(Font!, Text, Position);
            if (Texture is not null) drawingContext.Draw(Texture, Position);
            if (DrawRectangle) drawingContext.DrawRectangle(Rectangle, Color.White, 1);
        }
    }

    /// <summary>
    /// The data element relevant for update methods. <i>(Knowing when an <see cref="UIElement"/> is clicked.)</i>
    /// </summary>
    public struct UIElementUpdateData
    {
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is hovered by the mouse.
        /// </summary>
        public bool IsHovered { get; }
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is pressed by the mouse.
        /// </summary>
        public bool IsPressed(IInputManager input) => IsHovered && input.IsPressed(MouseButton.Left);
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is clicked by the mouse.
        /// </summary>
        public bool IsClicked(IInputManager input) => IsPressed(input) && input.IsClicked(MouseButton.Left);
        public bool IsEmpty => Source == null;
        public UIElement? Source { get; }
        /// <summary>
        /// Create a new <see cref="UIElementUpdateData"/> with the propeties <see cref="IsHovered"/> and <see cref="Source"/> set.
        /// </summary>
        /// <param name="isHovered">A value indicating if the source <see cref="UIElement"/> is hovered by the mouse.</param>
        public UIElementUpdateData(bool isHovered, UIElement? source)
        {
            IsHovered = isHovered;
            Source = source;
        }

        public static UIElementUpdateData Empty = new UIElementUpdateData(false, null);
    }
}
