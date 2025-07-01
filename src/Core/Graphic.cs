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
    public required Vector2 Position { get; init; }
    public required TOrderProvider OrderProvider { get; init; }
    public required UIPath Path { get; init; }

    public IDictionary<string, UIElementUpdateData> UpdateData => _updateData;
    public ReadOnlySpan<UIElement> Elements => _elements;

    private readonly UIElement[] _elements;
    private readonly UIElementDrawData[] _drawDump;
    private readonly IDictionary<string, UIElementUpdateData> _updateData;
    private readonly Dictionary<string, UIElement> _elementMap;

    public OrderContainer(TOrderProvider orderProvider, Vector2 position, IEnumerable<UIElement> elements, IDictionary<string, UIElementUpdateData>? updateData = null, UIPath? path = null)
    {
        OrderProvider = orderProvider;
        Position = position;
        _elements = elements.ToArray();
        _drawDump = new UIElementDrawData[_elements.Length];

        _updateData = updateData ?? STOLON.UI.UpdateData;
        _elementMap = _elements.ToDictionary(e => e.Id);
        Path = path ?? new UIPath([UIElement.TOP_ID]);
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
        return path.Segments.Length <= 1 ? new UIPath([]) : new UIPath(path.Segments[..^1].ToArray());
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
