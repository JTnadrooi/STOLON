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
    public readonly struct SelectionInfo
    {
        public ReadOnlyDictionary<string, SelectionEntry> Entries { get; }
        public int? TotalVAllocation { get; }
        public bool IsPostAllocation { get; }
        public int Count => Entries.Count;

        public SelectionInfo(SelectionEntry[] entries, bool isPostAllocation)
        {
            Entries = entries.ToDictionary(e => e.Entity.Id).AsReadOnly();

            int total = 0;
            foreach (SelectionEntry e in entries)
            {
                if (e.IsPostAllocation != isPostAllocation) throw new Exception("Invalid PostAllocation for entry: " + e);
                if (isPostAllocation) total += e.VAllocation!.Value;
            }
            TotalVAllocation = total == 0 ? null : total;
            IsPostAllocation = isPostAllocation;
        }
        public bool IsSelected(string id) => Entries.ContainsKey(id);
        public int GetAllocation(string id) => Entries.TryGetValue(id, out SelectionEntry entry) ? entry.Allocation : 0;
        public int GetVirtualAllocation(string id) => IsPostAllocation ? (Entries.TryGetValue(id, out SelectionEntry entry) ? entry.VAllocation!.Value : 0) : throw new InvalidOperationException();

        public static SelectionInfo Empty { get; } = new SelectionInfo(Array.Empty<SelectionEntry>(), true);
        public override string ToString() => $"IsPostAllocation: {IsPostAllocation}, TotalVAllocation: {(IsPostAllocation ? TotalVAllocation : "<n/a>")}, Entries: [{string.Join(", ", Entries.Values.Select(e => e.ToString()))}]";
    }
}
