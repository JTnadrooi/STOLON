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
        public int? VAllocation { get; }
        public bool IsPostAllocation => VAllocation.HasValue;

        public SelectionEntry(Entity entity, int alloc, int? valloc = null)
        {
            Entity = entity;
            Allocation = alloc;
            VAllocation = valloc;
        }
        public override string ToString() => $"Entity: {Entity.Id}, Allocation: {Allocation}, VAllocation: {(IsPostAllocation ? VAllocation : "<n/a>")}";
    }
    public class EntitySelection
    {
        public ReadOnlyDictionary<string, SelectionEntry> Entries { get; }
        public int? TotalVAllocation { get; }
        public bool IsPostAllocation { get; private set; }
        public int Count => Entries.Count;
        public int MaxEntries { get; }

        private readonly Dictionary<string, SelectionEntry> _entries;
        private readonly List<Entity> _toParseEntries;

        public SelectionEntry this[string id] => Entries[id];
        public SelectionEntry this[int i] => _entries.ElementAt(i).Value;

        public EntitySelection(int maxEntries)
        {
            _entries = new Dictionary<string, SelectionEntry>(maxEntries);
            Entries = _entries.AsReadOnly();
            _toParseEntries = new List<Entity>(maxEntries);

            TotalVAllocation = 0;
            IsPostAllocation = true;
            MaxEntries = maxEntries;
            //int total = 0;
            //foreach (SelectionEntry e in _entries.Values)
            //{
            //    if (e.IsPostAllocation != isPostAllocation) throw new Exception("Invalid PostAllocation for entry: " + e);
            //    if (isPostAllocation) total += e.VAllocation!.Value;
            //}
            //TotalVAllocation = total == 0 ? null : total;
            //IsPostAllocation = isPostAllocation;
        }
        public void Add(string id)
        {
            STOLON.Debug.Log(">selecting entity " + id + ".");
            _toParseEntries.Add(STOLON.Environment.Entities[id]);
            RecalculateAllocations();
            STOLON.Debug.Success();
        }
        public void Remove(string id)
        {
            STOLON.Debug.Log(">deselecting entity " + id + ".");
            _toParseEntries.Remove(STOLON.Environment.Entities[id]);
            RecalculateAllocations();
            STOLON.Debug.Success();
        }
        public bool Contains(string id)
        {
            return Entries.ContainsKey(id);
        }
        public int GetSlot(string id) => Entries.GetFirstIndexWhere(s => s.Key == id);
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

            STOLON.Debug.Success();
        }
        public int GetAllocation(string id) => _entries.TryGetValue(id, out SelectionEntry entry) ? entry.Allocation : -2;
        public int GetVirtualAllocation(string id) => IsPostAllocation ? (_entries.TryGetValue(id, out SelectionEntry entry) ? entry.VAllocation!.Value : -1) : throw new InvalidOperationException();

        public override string ToString() => $"IsPostAllocation: {IsPostAllocation}, TotalVAllocation: {(IsPostAllocation ? TotalVAllocation : "<n/a>")}, Entries: [{string.Join(", ", Entries.Values.Select(e => e.ToString()))}]";
    }
}
