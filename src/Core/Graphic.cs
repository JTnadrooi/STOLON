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
    public abstract class OrderContainer<TOrderProvider> : IGraphic where TOrderProvider : IOrderProvider
    {
        public Vector2 Position { get; set; }

        protected TOrderProvider OrderProvider { get; }
        protected UIElement[] Elements => _elements;
        protected UIElementDrawData[] DrawDump => _drawDump;
        protected IReadOnlyDictionary<string, UIElementUpdateData> UpdateData => _updateDump;

        private UIElement[] _elements;
        private UIElementDrawData[] _drawDump;
        private Dictionary<string, UIElementUpdateData> _updateDump;

        public OrderContainer(TOrderProvider orderProvider, Vector2 position, IEnumerable<UIElement> elements)
        {
            OrderProvider = orderProvider;
            Position = position;
            _updateDump = new Dictionary<string, UIElementUpdateData>();
            _elements = elements.ToArray();
            _drawDump = new UIElementDrawData[_elements.Length];
        }

        public void Update(int elapsedMilliseconds)
        {
            UIOrdering.Order(_elements!, "_", _drawDump!, _updateDump, Position, OrderProvider);
        }

        public void Draw(DrawingContext drawingContext)
        {
            foreach (UIElementDrawData elementDrawData in _drawDump)
                drawingContext.DrawElement(elementDrawData);
        }
    }
}
