using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;



namespace STOLON
{

    /// <summary>
    /// A class to allow objects to state a way of ordering <see cref="UIElement"/> objects.
    /// </summary>
    public interface IOrderProvider
    {
        public UIElementDrawData GetElementDrawData(UIElement element, Vector2 UIOrgin, int index, out bool isHovered);
    }

    /// <summary>
    /// The <see cref="IOrderProvider"/> that orders the main menu.
    /// </summary> 
    public class MenuOrderProvider : IOrderProvider
    {
        private Font2D _font;
        private bool _capitalise = true;
        public MenuOrderProvider()
        {
            _font = STOLON.Fonts.Medium;
        }
        public UIElementDrawData GetElementDrawData(UIElement element, Vector2 UIOrgin, int index, out bool isHovered)
        {
            Vector2 elementPos = Centering.CenterX((int)_font.FastMeasure(element.Text).X,
                                index * (-_font.Dimensions.Y * 2 - 2) + UIOrgin.Y,
                                STOLON.V_WIDTH, Vector2.One);
            Centering.OnPixel(ref elementPos);

            Rectangle elementBounds = new Rectangle(elementPos.ToPoint(), new Point((int)_font.FastMeasure(element.Text).X, (int)_font.Dimensions.Y));
            string elementText = element.Text;
            if (_capitalise) elementText = elementText.ToUpper();

            string postPre = element.Id switch
            {
                "quit" => "x",
                "specialThanks" => "!",
                _ => ">",
            };
            isHovered = elementBounds.Contains(STOLON.Input.VirtualMousePos);
            return new UIElementDrawData(element, isHovered
                ? (postPre + " " + elementText + " " + postPre.Replace(">", "<"))
                : elementText, STOLON.Fonts.Medium, element.Type, elementPos + (isHovered ? new Point(-(int)_font.FastMeasure(2).X, 0) : Point.Zero).ToVector2(), Rectangle.Empty, false);
        }
    }

    /// <summary>
    /// Provides methods for ordering <see cref="UIElement"/> objects. (Casting them to <see cref="UIElementDrawData"/> or/and <see cref="UIElementUpdateData"/>.
    /// </summary>
    public static class UIOrdering
    {
        public static void Order(UIElement[] uIElements, UIPath path, UIElementDrawData[] drawDump, IDictionary<string, UIElementUpdateData> updateDump,
            Vector2 uiOrgin, IOrderProvider orderProvider, bool isMouseRelevant = true)
        {
            Order(uIElements, path.DestinationId, drawDump, updateDump, uiOrgin, orderProvider, isMouseRelevant);
        }
        public static void Order(UIElement[] uIElements, string parentId, UIElementDrawData[] drawDump, IDictionary<string, UIElementUpdateData> updateDump,
            Vector2 uiOrgin, IOrderProvider orderProvider, bool isMouseRelevant = true)
        {
            int orderIndex = 0;
            updateDump.Clear();
            if (drawDump.Length != uIElements.Length) throw new ArgumentException("Invalid dump size.");
            for (int i = 0; i < uIElements.Length; i++)
            {
                UIElement element = uIElements[i];
                if (element.ParentId != parentId) continue;

                UIElementDrawData drawData = orderProvider.GetElementDrawData(element, uiOrgin, orderIndex++, out bool isHovered);
                updateDump[element.Id] = new UIElementUpdateData(isHovered && isMouseRelevant, element);
                drawDump[i] = drawData;
            }
        }
    }
}
