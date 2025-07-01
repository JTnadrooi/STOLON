using AsitLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public TOrderProvider OrderProvider { get; }
        public UIPath Path { get; set; }

        protected UIElement[] Elements => _elements;
        protected UIElementDrawData[] DrawDump => _drawDump;

        private UIElement[] _elements;
        private UIElementDrawData[] _drawDump;
        private IDictionary<string, UIElementUpdateData> _updateData;

        public OrderContainer(TOrderProvider orderProvider, Vector2 position, IEnumerable<UIElement> elements, IDictionary<string, UIElementUpdateData>? updateData = null, UIPath? path = null)
        {
            OrderProvider = orderProvider;
            Position = position;
            _elements = elements.ToArray();
            _drawDump = new UIElementDrawData[_elements.Length];
            _updateData = updateData ?? STOLON.UI.UpdateData;
            Path = path ?? new UIPath(new string[] { UIElement.TOP_ID });
        }

        public UIPath GetSelfPath(string id)
        {
            IEnumerable<string> GetListPath(string idForSearch)
                => idForSearch == UIElement.TOP_ID ? idForSearch.ToSingleArray() : GetListPath(_elements[idForSearch].Parent).Concat(idForSearch.ToSingleArray());
            return new UIPath(GetListPath(id).ToArray()[1..]);
        }
        public UIPath GetParentPath(string id) => new UIPath(GetSelfPath(id).Segments.ToArray()[..^1]);

        public virtual void Update(int elapsedMilliseconds)
        {
            UIOrdering.Order(_elements!, UIElement.TOP_ID, _drawDump, _updateData, Position, OrderProvider);
        }

        public virtual void Draw(DrawingContext drawingContext)
        {
            foreach (UIElementDrawData elementDrawData in _drawDump)
                drawingContext.DrawElement(elementDrawData);
        }
    }
}
