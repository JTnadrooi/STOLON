using AsitLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public readonly struct SelectionEntry
    {
        public Entity Entity { get; }
        public int Allocation { get; }
        public int VAllocation => IsPostAllocation ? _valloc : throw new InvalidOperationException();
        public bool IsPostAllocation { get; }

        private readonly int _valloc;

        public SelectionEntry(Entity entity, int alloc, int? valloc = null)
        {
            Entity = entity;
            Allocation = alloc;
            IsPostAllocation = valloc.HasValue;
            _valloc = valloc ?? -1;
        }
        public override string ToString() => $"Entity: {Entity.Id}, Allocation: {Allocation}, VAllocation: {(IsPostAllocation ? VAllocation : "<n/a>")}";
    }

    public class EntitySelection
    {
        public ReadOnlyDictionary<string, SelectionEntry> Entries { get; }
        public int TotalVAllocation => IsPostAllocation ? _totalVAllocation : throw new InvalidOperationException();
        public bool IsPostAllocation { get; private set; }
        public int Count => Entries.Count;
        public int MaxEntries { get; }

        private readonly Dictionary<string, SelectionEntry> _entries;
        private readonly List<Entity> _toParseEntries;
        private int _totalVAllocation;

        public SelectionEntry this[string id] => Entries[id];
        public SelectionEntry this[int i] => _entries[_toParseEntries[i].Id];

        public EntitySelection(int maxEntries)
        {
            _entries = new Dictionary<string, SelectionEntry>(maxEntries);
            Entries = _entries.AsReadOnly();
            _toParseEntries = new List<Entity>(maxEntries);
            _totalVAllocation = 0;
            IsPostAllocation = true;
            MaxEntries = maxEntries;
        }
        public bool Add(string id)
        {
            STOLON.Debug.Log(">selecting entity " + id + ".");
            if (Contains(id))
            {
                STOLON.Debug.Fail();
                return false;
            }
            _toParseEntries.Add(STOLON.Environment.Entities[id]);
            RecalculateAllocations();
            STOLON.Debug.Success();
            return true;
        }
        public bool Remove(string id)
        {
            STOLON.Debug.Log(">deselecting entity " + id + ".");
            if (!Contains(id))
            {
                STOLON.Debug.Fail();
                return false;
            }
            _toParseEntries.Remove(STOLON.Environment.Entities[id]);
            RecalculateAllocations();
            STOLON.Debug.Success();
            return true;
        }
        public bool Contains(string id)
        {
            return Entries.ContainsKey(id);
        }
        public int GetSlot(string id) => _toParseEntries.GetFirstIndexWhere(e => e.Id == id);
        private void RecalculateAllocations()
        {
            STOLON.Debug.Log(">updating allocations..");

            IsPostAllocation = false;
            _entries.Clear();
            for (int i = 0; i < _toParseEntries.Count; i++) // create source without virtual values.
                _entries.Add(_toParseEntries[i].Id, new SelectionEntry(_toParseEntries[i], 100 / _toParseEntries.Count));

            Stack<SelectionEntry> _entryBuffer = new Stack<SelectionEntry>(MaxEntries);
            for (int i = 0; i < _toParseEntries.Count; i++) // create _entryBuffer with virtual values. (using _entries referenced in GetVirtualAllocation())
                _entryBuffer.Push(new SelectionEntry(_toParseEntries[i], 100 / _toParseEntries.Count, _toParseEntries[i].GetVirtualAllocation(this)));

            _entries.Clear();
            for (int i = 0; i < _toParseEntries.Count; i++) // make buffer the new source without ref change.
                _entries.Add(_entryBuffer.Peek().Entity.Id, _entryBuffer.Pop());

            IsPostAllocation = true;

            _totalVAllocation = _entries.Sum(e => e.Value.VAllocation);

            STOLON.Debug.Success();
        }
        public int GetAllocation(string id) => _entries.TryGetValue(id, out SelectionEntry entry) ? entry.Allocation : 0;
        public int GetVirtualAllocation(string id) => IsPostAllocation ? (_entries.TryGetValue(id, out SelectionEntry entry) ? entry.VAllocation : 0) : throw new InvalidOperationException();

        public override string ToString() => $"IsPostAllocation: {IsPostAllocation}, TotalVAllocation: {(IsPostAllocation ? TotalVAllocation : "<n/a>")}, Entries: [{string.Join(", ", Entries.Values.Select(e => e.ToString()))}]";
    }
}
