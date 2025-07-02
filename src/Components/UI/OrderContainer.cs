using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.UIElement;

namespace STOLON
{
    public class OrderContainer<TOrderProvider> : IGraphic where TOrderProvider : IOrderProvider
    {
        public Vector2 Position { get; set; }
        public TOrderProvider OrderProvider { get; }
        public UIPath Path { get; protected set; }

        public IDictionary<string, UIElementUpdateData> UpdateData => _updateDump;
        public ReadOnlySpan<UIElement> Elements => _elements;
        public IReadOnlySet<string> Parents => _parents;
        public UIElementUpdateData this[string elementId] => UpdateData[elementId];

        private readonly UIElement[] _elements;
        private readonly UIElementDrawData[] _drawDump;
        private readonly IDictionary<string, UIElementUpdateData> _updateDump;
        private readonly Dictionary<string, UIElement> _elementMap;
        private readonly HashSet<string> _parents;

        public OrderContainer(TOrderProvider orderProvider, IEnumerable<UIElement> elements, Vector2 position, IDictionary<string, UIElementUpdateData>? updateData = null, UIPath? path = null)
        {
            List<UIElement> tempElements = new List<UIElement>(elements);

            OrderProvider = orderProvider;
            Position = position;

            _parents = new HashSet<string>(tempElements.Where(e => tempElements.Any(e2 => e2.Parent == e.Id)).Select(e => e.Id));
            foreach (string id in _parents) tempElements.Add(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));
            _elements = tempElements.ToArray();

            _drawDump = new UIElementDrawData[_elements.Length];
            _updateDump = updateData ?? STOLON.UI.UpdateDump;
            _elementMap = _elements.ToDictionary(e => e.Id);
            Path = path ?? GetSelfPath(UIElement.TOP_ID);
        }

        public UIPath GetSelfPath(string id)
        {
            if (id == UIElement.TOP_ID) return new UIPath([UIElement.TOP_ID]);

            Stack<string> stack = new Stack<string>();
            string currentId = id;

            while (true)
            {
                stack.Push(currentId);
                if (!_elementMap.TryGetValue(currentId, out var element)) throw new InvalidOperationException($"Element with id '{currentId}' not found.");
                if (element.Parent == UIElement.TOP_ID)
                {
                    stack.Push(UIElement.TOP_ID);
                    break;
                }
                currentId = element.Parent;
            }
            return new UIPath(stack);
        }
        public UIPath GetParentPath(string id) => id == UIElement.TOP_ID ? throw new InvalidOperationException("Id equal to UIElement.TOP_ID") : new UIPath(GetSelfPath(id).Segments[..^1].ToArray());
        public virtual void Update(int elapsedMilliseconds)
        {
            for (int i = 0; i < _drawDump.Length; i++) _drawDump[i] = UIElementDrawData.Empty;
            UIOrdering.Order(_elements, Path, _drawDump, _updateDump, Position, OrderProvider);
            foreach (UIElementUpdateData data in _updateDump.Values)
            {
                if (data.IsClicked)
                {
                    if (data.Source.Id.StartsWith("_back_"))
                        Path = GetParentPath(data.Source.Id.Substring("_back_".Length));
                    else if (_parents.Contains(data.Source.Id))
                        Path = GetSelfPath(data.Source.Id);
                    STOLON.Debug.Log("element clicked: " + data.Source.Id);
                }
            }
        }

        public virtual void Draw(DrawingContext drawingContext)
        {
            foreach (UIElementDrawData elementDrawData in _drawDump)
            {
                if (elementDrawData.Id == null) continue;
                drawingContext.DrawElement(elementDrawData);
            }
        }
    }
}
