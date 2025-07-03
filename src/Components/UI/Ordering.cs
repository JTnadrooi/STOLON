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
