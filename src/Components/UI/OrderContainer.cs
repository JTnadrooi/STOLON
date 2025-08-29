using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.UIElement;

namespace STOLON
{
    public abstract class OrderContainer : IGraphic
    {
        public Vector2 Position { get; set; }
        public UIPath Path { get; protected set; }

        public IDictionary<string, UIElementUpdateData> UpdateData => _updateDump;
        public IReadOnlyDictionary<string, UIElement> Elements => _elementMap;
        public IReadOnlySet<string> Parents => _parents;
        public UIElementUpdateData this[string elementId] => UpdateData[elementId];

        private readonly UIElement[] _elements;
        private readonly UIElementDrawData[] _drawDump;
        private readonly IDictionary<string, UIElementUpdateData> _updateDump;
        private readonly Dictionary<string, UIElement> _elementMap;
        private readonly HashSet<string> _parents;

        public OrderContainer(IEnumerable<UIElement> elements, Vector2? position = null, IDictionary<string, UIElementUpdateData>? updateData = null, UIPath? path = null)
        {
            List<UIElement> tempElements = new List<UIElement>(elements);

            Position = position ?? Vector2.Zero;

            _parents = new HashSet<string>(tempElements.Where(e => tempElements.Any(e2 => e2.ParentId == e.Id)).Select(e => e.Id));
            foreach (string id in _parents) tempElements.Add(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));
            _elements = tempElements.ToArray();

            _drawDump = new UIElementDrawData[_elements.Length];
            _updateDump = updateData ?? STOLON.UI.UpdateDump;
            _elementMap = _elements.ToDictionary(e => e.Id);
            Path = path ?? GetSelfPath(UIElement.TOP_ID);
        }

        public virtual void PrepareOrdering(Vector2 origin, int elementCount) { }
        public abstract UIElementDrawData GetDrawData(UIElement element, int index, out bool isHovered);
        public void AfterOrdering() { }


        public UIPath GetSelfPath(string id)
        {
            if (id == UIElement.TOP_ID) return new UIPath([UIElement.TOP_ID]);

            Stack<string> stack = new Stack<string>();
            string currentId = id;

            while (true)
            {
                stack.Push(currentId);
                if (!_elementMap.TryGetValue(currentId, out var element)) throw new InvalidOperationException($"Element with id '{currentId}' not found.");
                if (element.ParentId == UIElement.TOP_ID)
                {
                    stack.Push(UIElement.TOP_ID);
                    break;
                }
                currentId = element.ParentId;
            }
            return new UIPath(stack);
        }
        public UIPath GetParentPath(string id) => id == UIElement.TOP_ID ? throw new InvalidOperationException("Id equal to UIElement.TOP_ID") : new UIPath(GetSelfPath(id).Segments[..^1].ToArray());
        public virtual void Update(int elapsedMilliseconds)
        {
            for (int i = 0; i < _drawDump.Length; i++) _drawDump[i] = UIElementDrawData.Empty;

            _updateDump.Clear();

            int orderIndex = 0;
            if (_drawDump.Length != _elements.Length) throw new ArgumentException("Invalid dump size.");

            PrepareOrdering(Position, _elements.Count(e => !e.Skip));
            for (int i = 0; i < _elements.Length; i++)
            {
                UIElement element = _elements[i];
                if (element.Skip || element.ParentId != Path.DestinationId)
                    continue;

                UIElementDrawData drawData = GetDrawData(element, orderIndex++, out bool isHovered);
                _updateDump[element.Id] = new UIElementUpdateData(isHovered, element);
                _drawDump[i] = drawData;
            }
            AfterOrdering();

            foreach (UIElementUpdateData data in _updateDump.Values)
                if (data.IsClicked)
                {
                    if (data.Source.Id.StartsWith("_back_"))
                        Path = GetParentPath(data.Source.Id.Substring("_back_".Length));
                    else if (_parents.Contains(data.Source.Id))
                        Path = GetSelfPath(data.Source.Id);

                    STOLON.Debug.Log("element clicked: " + data.Source.Id);
                }
        }


        public virtual void Draw(DrawingContext drawingContext)
        {
            foreach (UIElementDrawData elementDrawData in _drawDump)
                if (elementDrawData.Source == null) continue;
                else drawingContext.DrawElement(elementDrawData);
        }
    }
}
