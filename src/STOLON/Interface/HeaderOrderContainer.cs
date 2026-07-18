namespace STOLON
{
    public class HeaderOrderContainer : OrderContainer<TextElement>
    {
        private Vector2 _origin;

        private int _leftSpace;

        private const int PaddingX = 8;
        private const int PaddingY = 4;

        public HeaderOrderContainer(IInputManager input, IEnumerable<TextElement> elements, Vector2? position = null) : base(input, elements, position)
        {
            _leftSpace = 0;
        }

        public override void PrepareOrdering(Vector2 origin, int elementCount)
        {
            _origin = origin;
            _leftSpace = 0;
        }

        public override UIElementDrawData GetDrawData(TextElement element, int index, out bool isHovered)
        {
            Vector2 pos = _origin + new Vector2(_leftSpace, 0);
            Rectangle bounds = element.GetBounds(pos.ToPoint(), PaddingX, PaddingY, 5, (int)(32 / 2 - element.Font.Dimensions.Y / 2 - PaddingY));
            Point textPos = element.GetTextPos(bounds, PaddingX, PaddingY);
            _leftSpace += bounds.Width + 5;

            isHovered = false;
            return new UIElementDrawData(element, element.Text.ToUpper(), element.Font, element.Type, textPos.ToVector2(), bounds, true, false, true);
        }
    }
}
