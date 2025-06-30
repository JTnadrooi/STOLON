using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public interface IGraphic
    {
        public void Draw(DrawingContext drawingContext);
    }
    public class OrderContainer<TOrderProvider> : IGraphic where TOrderProvider : IOrderProvider
    {
        public Vector2 Position { get; set; }
        public IDictionary<string, UIElementUpdateData> UpdateData => _updateData;

        protected TOrderProvider OrderProvider { get; }
        protected UIElement[] Elements => _elements;
        protected UIElementDrawData[] DrawDump => _drawDump;

        private UIElement[] _elements;
        private UIElementDrawData[] _drawDump;
        private IDictionary<string, UIElementUpdateData> _updateData;

        public OrderContainer(TOrderProvider orderProvider, Vector2 position, IEnumerable<UIElement> elements, IDictionary<string, UIElementUpdateData>? updateData = null)
        {
            OrderProvider = orderProvider;
            Position = position;
            _elements = elements.ToArray();
            _drawDump = new UIElementDrawData[_elements.Length];
            _updateData = updateData ?? STOLON.UI.UpdateData;
        }

        public virtual void Update(int elapsedMilliseconds)
        {
            UIOrdering.Order(_elements!, UserInterface.HIGHEST_PARENT_ID, _drawDump, _updateData, Position, OrderProvider);
        }

        public virtual void Draw(DrawingContext drawingContext)
        {
            foreach (UIElementDrawData elementDrawData in _drawDump)
                drawingContext.DrawElement(elementDrawData);
        }
    }
}
