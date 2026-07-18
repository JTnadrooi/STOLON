using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace STOLON
{
    public abstract class OrderContainer<TUIElement> : IComponent where TUIElement : UIElement
    {
        public Vector2 Position { get; set; }
        public UIPath Path { get; protected set; }

        public Matrix Transform { get; set; }

        public IReadOnlyDictionary<string, UIElementUpdateData> UpdateData => _updateDataView;
        public IReadOnlyDictionary<string, TUIElement> Elements => _elementMap;
        public IReadOnlySet<string> Parents => _parents;

        public UIElementUpdateData this[string elementId] => UpdateData[elementId];

        private readonly TUIElement[] _elements;
        private readonly UIElementDrawData[] _drawDump;
        private readonly Dictionary<string, UIElementUpdateData> _updateDump;
        private readonly IReadOnlyDictionary<string, UIElementUpdateData> _updateDataView;
        private readonly Dictionary<string, TUIElement> _elementMap;
        private readonly HashSet<string> _parents;
        private readonly List<int> _visibleIndices;
        private readonly IInputManager _input;

        protected OrderContainer(
            IInputManager input,
            IEnumerable<TUIElement> elements,
            Vector2? position = null,
            IDictionary<string, UIElementUpdateData>? updateData = null,
            UIPath? path = null,
            Func<string, TUIElement>? backElementFactory = null)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            _input = input;

            List<TUIElement> baseElements = new List<TUIElement>();
            HashSet<string> idSet = new HashSet<string>();

            foreach (TUIElement element in elements)
            {
                if (!VerifyElement(element)) throw new ArgumentException($"Cannot add invalid element '{element.Id}'.", nameof(elements));

                baseElements.Add(element);
                idSet.Add(element.Id);
            }

            _parents = new HashSet<string>();

            for (int i = 0; i < baseElements.Count; i++)
                if (baseElements[i].ParentId != UIElement.TopId && idSet.Contains(baseElements[i].ParentId))
                    _parents.Add(baseElements[i].ParentId);

            if (backElementFactory is not null)
                foreach (string parentId in _parents) baseElements.Add(backElementFactory.Invoke(parentId));

            _elements = baseElements.ToArray();
            _drawDump = new UIElementDrawData[_elements.Length];
            _visibleIndices = new List<int>(_elements.Length);

            _updateDump = updateData?.ToDictionary() ?? new Dictionary<string, UIElementUpdateData>(_elements.Length);

            _updateDataView = new ReadOnlyDictionary<string, UIElementUpdateData>(_updateDump);

            _elementMap = new Dictionary<string, TUIElement>(_elements.Length);
            for (int i = 0; i < _elements.Length; i++) _elementMap[_elements[i].Id] = _elements[i];

            Position = position ?? Vector2.Zero;
            Path = path ?? UIPath.TopPath;
        }

        public virtual void PrepareOrdering(Vector2 origin, int visibleElementCount) { }
        public abstract UIElementDrawData GetDrawData(TUIElement element, int orderIndex, out bool isHovered);
        public virtual void AfterOrdering() { }
        protected virtual void OnPathChanged(UIPath previous, UIPath current) { }
        protected virtual bool VerifyElement(TUIElement element)
        {
            return true;
        }

        public UIPath GetSelfPath(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (id == UIElement.TopId) return UIPath.TopPath;

            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
            Stack<string> stack = new Stack<string>();
            string currentId = id;

            while (true)
            {
                if (!visited.Add(currentId)) throw new InvalidOperationException($"Cycle detected while resolving path for '{id}'.");
                stack.Push(currentId);
                if (!_elementMap.TryGetValue(currentId, out TUIElement? element)) throw new InvalidOperationException($"Element with id '{currentId}' not found.");

                if (element.ParentId == UIElement.TopId)
                {
                    stack.Push(UIElement.TopId);
                    break;
                }

                currentId = element.ParentId;
            }

            return new UIPath(stack);
        }

        public UIPath GetParentPath(string id)
        {
            if (id == UIElement.TopId) throw new InvalidOperationException("Id equal to UIElement.TOP_ID has no parent.");

            ReadOnlySpan<string> segments = GetSelfPath(id).Segments;

            if (segments.Length <= 1) throw new InvalidOperationException($"Element '{id}' has no parent path.");

            string[] parentSegments = new string[segments.Length - 1];
            segments.Slice(0, segments.Length - 1).CopyTo(parentSegments);

            return new UIPath(parentSegments);
        }

        public bool TryGetUpdate(string elementId, out UIElementUpdateData data) => _updateDump.TryGetValue(elementId, out data);

        public virtual void Update(int elapsedMilliseconds)
        {
            Array.Clear(_drawDump, 0, _drawDump.Length);
            _updateDump.Clear();
            _visibleIndices.Clear();

            string current = Path.DestinationId;

            for (int i = 0; i < _elements.Length; i++)
                if (!_elements[i].Skip && _elements[i].ParentId == current)
                    _visibleIndices.Add(i);

            PrepareOrdering(Position, _visibleIndices.Count);

            int orderIndex = 0;
            for (int i = 0; i < _elements.Length; i++)
            {
                TUIElement element = _elements[i];
                _updateDump[element.Id] = new UIElementUpdateData(false, element);
            }
            for (int i = 0; i < _visibleIndices.Count; i++)
            {
                int idx = _visibleIndices[i];
                TUIElement element = _elements[idx];
                _drawDump[idx] = GetDrawData(element, orderIndex++, out bool isHovered);

                _updateDump[element.Id] = new UIElementUpdateData(isHovered, element);
            }

            string? clickedId = null;
            for (int v = _visibleIndices.Count - 1; v >= 0; v--)
            {
                int idx = _visibleIndices[v];
                string id = _elements[idx].Id;

                if (_updateDump.TryGetValue(id, out UIElementUpdateData data) && data.IsClicked(_input))
                {
                    clickedId = id;
                    break;
                }
            }

            if (clickedId != null)
            {
                UIPath oldPath = Path;

                if (clickedId.Length > UIElement.BackPrefix.Length && clickedId.StartsWith(UIElement.BackPrefix)) Path = GetParentPath(clickedId.Substring(UIElement.BackPrefix.Length));
                else if (_parents.Contains(clickedId)) Path = GetSelfPath(clickedId);

                if (!Equals(oldPath, Path))
                {
                    OnPathChanged(oldPath, Path);
                }
            }

            AfterOrdering();
        }

        public virtual void Draw(DrawingContext drawingContext)
        {
            for (int i = 0; i < _drawDump.Length; i++)
            {
                UIElementDrawData drawData = _drawDump[i];
                if (drawData == null) continue;

                if (_drawDump[i].Source != null) drawData.Draw(drawingContext);
                drawingContext.RegisterDraw(drawData, drawData.Rectangle, Transform);
            }
        }
    }

}
