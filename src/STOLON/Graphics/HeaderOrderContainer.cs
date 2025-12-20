namespace STOLON
{
    public class HeaderOrderContainer : OrderContainer
    {
        private Font2D _font;
        private Vector2 _origin;

        private int _leftSpace;

        private const int PADDING_X = 8;
        private const int PADDING_Y = 4;

        public HeaderOrderContainer(IEnumerable<UIElement> elements, Vector2? position = null) : base(elements, position)
        {
            _font = STOLON.Fonts.Medium;
            _leftSpace = 0;
        }

        public override void PrepareOrdering(Vector2 origin, int elementCount)
        {
            _origin = origin;
            _leftSpace = 0;
        }

        public override UIElementDrawData GetDrawData(UIElement element, int index, out bool isHovered)
        {
            Vector2 pos = _origin + new Vector2(_leftSpace, 0);
            Rectangle bounds = element.GetBounds(pos.ToPoint(), PADDING_X, PADDING_Y, 5, (int)(32 / 2 - _font.Dimensions.Y / 2 - PADDING_Y), out Point textPos);
            _leftSpace += bounds.Width + 5;

            isHovered = false;
            return new UIElementDrawData(element, element.Text.ToUpper(), _font, element.Type, textPos.ToVector2(), bounds, true, false, true);
        }
    }
}
