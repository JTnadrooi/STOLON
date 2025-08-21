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
            _toParseEntries = new List<Entity>(4);

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
            for (int i = 0; i < _toParseEntries.Count; i++)
                _entries.Add(_toParseEntries[i].Id, new SelectionEntry(_toParseEntries[i], 100 / _toParseEntries.Count));

            _entries.Clear();
            for (int i = 0; i < _toParseEntries.Count; i++)
                _entries.Add(_toParseEntries[i].Id, new SelectionEntry(_toParseEntries[i], 100 / _toParseEntries.Count, _toParseEntries[i].GetVirtualAllocation(this)));
            IsPostAllocation = true;

            STOLON.Debug.Success();
        }

        public bool IsSelected(string id) => Entries.ContainsKey(id);
        public int GetAllocation(string id) => Entries.TryGetValue(id, out SelectionEntry entry) ? entry.Allocation : 0;
        public int GetVirtualAllocation(string id) => IsPostAllocation ? (Entries.TryGetValue(id, out SelectionEntry entry) ? entry.VAllocation!.Value : 0) : throw new InvalidOperationException();

        public override string ToString() => $"IsPostAllocation: {IsPostAllocation}, TotalVAllocation: {(IsPostAllocation ? TotalVAllocation : "<n/a>")}, Entries: [{string.Join(", ", Entries.Values.Select(e => e.ToString()))}]";
    }
}
