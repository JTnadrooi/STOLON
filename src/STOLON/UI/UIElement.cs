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
    /// Reprecents a button or textplane in the UI. Add new elements to the <see cref="Interface"/> using the <see cref="Interface.AddElement(UIElement)"/> method.
    /// </summary>
    public class UIElement
    {
        public bool IsTop => ParentId == TOP_ID;
        public const string TOP_ID = "_";
        /// <summary>
        /// The type of the <see cref="UIElement"/>.
        /// </summary>
        public UIElementType Type { get; }
        /// <summary>
        /// The text in this <see cref="UIElement"/>.
        /// </summary>
        public string Text { get; set; }
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

        public object?[] DrawArguments { get; }

        public const int DEFAULT_RECTANGLE_CLEARANCE = 2;

        public UIElement(string id, string parentId = UIElement.TOP_ID, string? text = null, UIElementType type = UIElementType.Listen, string? order = null, CachedAudio? clickSound = null, params object?[] drawArgs)
        {
            Text = text ?? id;
            Type = type;
            Id = id;
            Order = order;
            ParentId = parentId;
            DrawArguments = drawArgs;
            Skip = false;
        }
        public Rectangle GetBounds(Point pos, int padding, int margin, Font2D font, out Point textPos) => GetBounds(pos, padding, padding, margin, margin, font, out textPos);
        public Rectangle GetBounds(Point pos, int paddingX, int paddingY, int marginX, int marginY, Font2D font, out Point textPos)
        {
            Vector2 contentSize = font.FastMeasure(Text);
            int recW = (int)contentSize.X + 2 * paddingX;
            int recH = (int)contentSize.Y + 2 * paddingY;

            textPos = new Point(pos.X + paddingX + marginX, pos.Y + paddingY + marginY);
            return new Rectangle(pos + new Point(marginX, marginY), new Point(recW, recH));
        }

        public override string ToString() => $"{{Id: {Id}, Type: {Type}, Text: {Text}, Order: {Order}, ParentId: {ParentId}}}";
    }
    /// <summary>
    /// The data element relevant for draw methods.
    /// </summary>
    public struct UIElementDrawData
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
        public string Text { get; }
        public UIElement? Source { get; }
        public bool IsEmpty => Source == null;
        public bool Hide { get; }
        public bool DrawBackground { get; }
        public Font2D Font { get; }
        /// <summary>
        /// Create a new <see cref="UIElementDrawData"/> object.
        /// </summary>
        /// <param name="sourceId">The source <see cref="UIElement.Id"/>.</param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <param name="rectangle"></param>
        /// <param name="drawRectangle"></param>
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

        public override string ToString() => $"{{Id: '{Source}\", Text: '{Text}\", Type: {Type}, Position: {Position}, Rectangle: {Rectangle}, DrawRectangle: {DrawRectangle}, Draw: {Hide}, Font: {Font?.ToString() ?? "null"}}}";
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
        public bool IsPressed(IInputManager input) => IsHovered && input.CurrentMouse.LeftButton == ButtonState.Pressed;
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is clicked by the mouse.
        /// </summary>
        public bool IsClicked(IInputManager input) => IsPressed(input) && input.PreviousMouse.LeftButton == ButtonState.Released;
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

    public static class UIElementDrawingExtensions
    {
        public static void DrawElement(this DrawingContext context, UIElementDrawData drawData)
        {
            if (drawData.Source == null) throw new InvalidOperationException();
            if (drawData.Hide) return;
            if (drawData.DrawBackground) context.DrawArea(drawData.Rectangle, Color.Black);
            context.DrawString(drawData.Font, drawData.Text, drawData.Position);
            if (drawData.DrawRectangle) context.DrawRectangle(drawData.Rectangle, Color.White, Interface.LINE_WIDTH);
        }
    }
}
