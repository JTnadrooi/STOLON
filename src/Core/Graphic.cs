using AsitLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static STOLON.UIElement;

namespace STOLON;

public interface IGraphic
{
    void Draw(DrawingContext drawingContext);
}

public class OrderContainer<TOrderProvider> : IGraphic where TOrderProvider : IOrderProvider
{
    public Vector2 Position { get; set; }
    public TOrderProvider OrderProvider { get; }
    public UIPath Path { get; protected set; }

    public IDictionary<string, UIElementUpdateData> UpdateData => _updateData;
    public ReadOnlySpan<UIElement> Elements => _elements;
    public IReadOnlySet<string> Parents => _parents;

    private readonly UIElement[] _elements;
    private readonly UIElementDrawData[] _drawDump;
    private readonly IDictionary<string, UIElementUpdateData> _updateData;
    private readonly Dictionary<string, UIElement> _elementMap;
    private readonly HashSet<string> _parents;

    public OrderContainer(TOrderProvider orderProvider, IEnumerable<UIElement> elements, Vector2 position, IDictionary<string, UIElementUpdateData>? updateData = null, UIPath? path = null)
    {
        List<UIElement> tempElements = new List<UIElement>(elements);

        OrderProvider = orderProvider;
        Position = position;
        _drawDump = new UIElementDrawData[_elements.Length];

        _updateData = updateData ?? STOLON.UI.UpdateData;
        _elementMap = tempElements.ToDictionary(e => e.Id);
        Path = path ?? GetSelfPath(UIElement.TOP_ID);

        _parents = new HashSet<string>(tempElements.Where(e => tempElements.Any(e2 => e2.Parent == e.Id)).Select(e => e.Id));
        foreach (string id in _parents) tempElements.Add(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));

        _elements = tempElements.ToArray();
    }

    public UIPath GetSelfPath(string id)
    {
        Stack<string> stack = new Stack<string>();
        string currentId = id;
        while (currentId != UIElement.TOP_ID)
        {
            if (!_elementMap.TryGetValue(currentId, out var element)) throw new InvalidOperationException($"Element with id '{currentId}' not found.");
            stack.Push(currentId);
            currentId = element.Parent;
        }
        return new UIPath(stack);
    }

    public UIPath GetParentPath(string id)
    {
        UIPath path = GetSelfPath(id);
        return new UIPath(path.Segments[..^1].ToArray());
    }

    public virtual void Update(int elapsedMilliseconds)
    {
        UIOrdering.Order(_elements, Path, _drawDump, _updateData, Position, OrderProvider);
        foreach (UIElementUpdateData data in _updateData.Values)
        {
            if (_parents.Contains(data.SourceId))
            {
                Path = GetSelfPath(data.SourceId);
            }
        }
    }

    public virtual void Draw(DrawingContext drawingContext)
    {
        foreach (UIElementDrawData elementDrawData in _drawDump) drawingContext.DrawElement(elementDrawData);
    }
}
