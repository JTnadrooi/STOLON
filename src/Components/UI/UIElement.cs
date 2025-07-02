using Microsoft.Xna.Framework;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using RectangleF = MonoGame.Extended.RectangleF;



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
    /// Reprecents a button or textplane in the UI. Add new elements to the <see cref="UserInterface"/> using the <see cref="UserInterface.AddElement(UIElement)"/> method.
    /// </summary>
    public class UIElement
    {
        public bool IsTop => ParentId == TOP_ID;
        public string ClickSoundID { get; }
        public CachedAudio ClickSound => STOLON.Audio.Library[ClickSoundID];
        public const string TOP_ID = "_";
        /// <summary>
        /// The type of the <see cref="UIElement"/>.
        /// </summary>
        public UIElementType Type { get; }
        /// <summary>
        /// The text in this <see cref="UIElement"/>.
        /// </summary>
        public string Text { get; set; }
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

        public UIElement(string id, string parentId = UIElement.TOP_ID, string? text = null, UIElementType type = UIElementType.Listen, string? order = null, string? clickSoundId = null, params object?[] drawArgs)
        {
            Text = text ?? id;
            Type = type;
            Id = id;
            Order = order;
            ParentId = parentId;
            DrawArguments = drawArgs;
            ClickSoundID = clickSoundId ?? "select3";
        }



        public override string ToString()
        {
            return "{" + $"Id={Id}, Type={Type}, Text={Text}, Order={Order}, ChildOf={ParentId}" + "}";
        }

        public const int DEFAULT_RECTANGLE_CLEARANCE = 2;
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
        public RectangleF Rectangle { get; }
        /// <summary>
        /// The type of the <see cref="UIElement"/>. Sometimes relevant for drawing.
        /// </summary>
        public UIElementType Type { get; }
        /// <summary>
        /// The text to draw inside the <see cref="Rectangle"/>.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// The <see cref="UIElement.Id"/> of the source <see cref="UIElement"/>.
        /// </summary>
        public string? Id { get; }
        public bool Hide { get; }
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
        public UIElementDrawData(string? sourceId, string text, Font2D font, UIElementType type, Vector2 position, RectangleF rectangle, bool drawRectangle, bool hide = false)
        {
            Position = position;
            Type = type;
            Text = text;
            Rectangle = rectangle;
            DrawRectangle = drawRectangle;
            Id = sourceId;
            Font = font;
            Hide = hide;
        }
        public override string ToString() => $"UIElementDrawData {{ Id: \"{Id}\", Text: \"{Text}\", Type: {Type}, Position: {Position}, Rectangle: {Rectangle}, DrawRectangle: {DrawRectangle}, Draw: {Hide}, Font: {Font?.ToString() ?? "null"} }}";

        public static UIElementDrawData Empty = new UIElementDrawData(null, string.Empty, STOLON.Fonts.Medium, UIElementType.Ignore, default, default, false, true);
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
        public bool IsPressed => IsHovered && STOLON.Input.CurrentMouse.LeftButton == ButtonState.Pressed;
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is clicked by the mouse.
        /// </summary>
        public bool IsClicked => IsPressed && STOLON.Input.PreviousMouse.LeftButton == ButtonState.Released;
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
