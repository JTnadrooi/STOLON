using AsitLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace STOLON;

public interface IGraphic
{
    void Draw(DrawingContext drawingContext);
}

public class OrderContainer<TOrderProvider> : IGraphic where TOrderProvider : IOrderProvider
{
    public Vector2 Position { get; set; }
    public TOrderProvider OrderProvider { get; }
    public UIPath Path { get; }

    public IDictionary<string, UIElementUpdateData> UpdateData => _updateData;
    public ReadOnlySpan<UIElement> Elements => _elements;
    public ReadOnlySpan<string> Gates => _gates;

    private readonly UIElement[] _elements;
    private readonly UIElementDrawData[] _drawDump;
    private readonly IDictionary<string, UIElementUpdateData> _updateData;
    private readonly Dictionary<string, UIElement> _elementMap;
    private readonly string[] _gates;

    public OrderContainer(TOrderProvider orderProvider, IEnumerable<UIElement> elements, Vector2 position, IDictionary<string, UIElementUpdateData>? updateData = null, UIPath? path = null)
    {
        OrderProvider = orderProvider;
        Position = position;
        _elements = elements.ToArray();
        _drawDump = new UIElementDrawData[_elements.Length];

        _updateData = updateData ?? STOLON.UI.UpdateData;
        _elementMap = _elements.ToDictionary(e => e.Id);
        Path = path ?? GetSelfPath(UIElement.TOP_ID);

        _gates = elements.Where(e => elements.Any(e2 => e2.Parent == e.Id)).Select(e => e.Id).ToArray();
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
        UIOrdering.Order(_elements, UIElement.TOP_ID, _drawDump, _updateData, Position, OrderProvider);
    }

    public virtual void Draw(DrawingContext drawingContext)
    {
        foreach (UIElementDrawData elementDrawData in _drawDump) drawingContext.DrawElement(elementDrawData);
    }
}
