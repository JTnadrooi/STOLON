namespace STOLON
{

    public abstract class OrderContainer : IComponent, IGraphic
    {
        private const string BackPrefix = "_back_";

        public Vector2 Position { get; set; }
        public UIPath Path { get; protected set; }

        public IReadOnlyDictionary<string, UIElementUpdateData> UpdateData => _updateDataView;
        public IReadOnlyDictionary<string, UIElement> Elements => _elementMap;
        public IReadOnlySet<string> Parents => _parents;

        public UIElementUpdateData this[string elementId] => UpdateData[elementId];

        private readonly UIElement[] _elements;
        private readonly UIElementDrawData[] _drawDump;
        private readonly Dictionary<string, UIElementUpdateData> _updateDump;
        private readonly IReadOnlyDictionary<string, UIElementUpdateData> _updateDataView;
        private readonly Dictionary<string, UIElement> _elementMap;
        private readonly HashSet<string> _parents;
        private readonly List<int> _visibleIndices;
        private readonly IInputManager _input;

        protected OrderContainer(IEnumerable<UIElement> elements, IInputManager input, Vector2? position = null, IDictionary<string, UIElementUpdateData>? updateData = null, UIPath? path = null)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            _input = input;

            List<UIElement> baseElements = new List<UIElement>();
            HashSet<string> idSet = new HashSet<string>();

            foreach (UIElement element in elements)
            {
                baseElements.Add(element);
                idSet.Add(element.Id);
            }

            _parents = new HashSet<string>();

            for (int i = 0; i < baseElements.Count; i++)
                if (baseElements[i].ParentId != UIElement.TopId && idSet.Contains(baseElements[i].ParentId))
                    _parents.Add(baseElements[i].ParentId);

            foreach (string parentId in _parents) baseElements.Add(new UIElement(BackPrefix + parentId, parentId, "Back", UIElementType.Listen));

            _elements = baseElements.ToArray();
            _drawDump = new UIElementDrawData[_elements.Length];
            _visibleIndices = new List<int>(_elements.Length);

            _updateDump = updateData?.ToDictionary() ?? new Dictionary<string, UIElementUpdateData>(_elements.Length);

            _updateDataView = new ReadOnlyDictionary<string, UIElementUpdateData>(_updateDump);

            _elementMap = new Dictionary<string, UIElement>(_elements.Length);
            for (int i = 0; i < _elements.Length; i++) _elementMap[_elements[i].Id] = _elements[i];

            Position = position ?? Vector2.Zero;
            Path = path ?? UIPath.TopPath;
        }

        public virtual void PrepareOrdering(Vector2 origin, int visibleElementCount) { }
        public abstract UIElementDrawData GetDrawData(UIElement element, int orderIndex, out bool isHovered);
        public virtual void AfterOrdering() { }
        protected virtual void OnPathChanged(UIPath previous, UIPath current) { }

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
                if (!_elementMap.TryGetValue(currentId, out UIElement? element)) throw new InvalidOperationException($"Element with id '{currentId}' not found.");

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
            for (int i = 0; i < _visibleIndices.Count; i++)
            {
                int idx = _visibleIndices[i];
                UIElement element = _elements[idx];
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

                if (clickedId.Length > BackPrefix.Length && clickedId.StartsWith(BackPrefix)) Path = GetParentPath(clickedId.Substring(BackPrefix.Length));
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
                if (_drawDump[i].Source != null) drawingContext.DrawElement(_drawDump[i]);
        }
    }

}
