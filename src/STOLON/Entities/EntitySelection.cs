using System.Collections;

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

    public sealed class EntitySelection : IReadOnlyDictionary<string, SelectionEntry>, IReadOnlyList<SelectionEntry>
    {
        public ReadOnlyDictionary<string, SelectionEntry> Entries { get; }
        public int TotalVAllocation => IsPostAllocation ? _totalVAllocation : throw new InvalidOperationException();
        public bool IsPostAllocation { get; private set; }
        public int Count => Entries.Count;
        public int MaxEntries { get; }
        public IEnumerable<string> Keys => Entries.Keys;
        public IEnumerable<SelectionEntry> Values => Entries.Values;

        private readonly Dictionary<string, SelectionEntry> _entries;
        private readonly List<Entity> _selectedEntities;
        private int _totalVAllocation;

        public SelectionEntry this[string id] => Entries[id];
        public SelectionEntry this[int i] => _entries[_selectedEntities[i].Id];

        private readonly ReadOnlyDictionary<string, Entity> _entities;

        public EntitySelection(Entity[] entities, int maxEntries)
        {
            _entities = new ReadOnlyDictionary<string, Entity>(entities.ToDictionary(e => e.Id));

            _entries = new Dictionary<string, SelectionEntry>(maxEntries);
            Entries = _entries.AsReadOnly();
            _selectedEntities = new List<Entity>(maxEntries);
            _totalVAllocation = 0;
            IsPostAllocation = true;
            MaxEntries = maxEntries;
        }

        private void RecalculateAllocations()
        {
            IsPostAllocation = false;

            _entries.Clear();
            for (int i = 0; i < _selectedEntities.Count; i++) // create source without virtual values.
                _entries.Add(_selectedEntities[i].Id, new SelectionEntry(_selectedEntities[i], 100 / _selectedEntities.Count));

            Stack<SelectionEntry> entryBuffer = new Stack<SelectionEntry>(MaxEntries);
            for (int i = 0; i < _selectedEntities.Count; i++) // create entryBuffer with virtual values. (using _entries referenced in GetVirtualAllocation())
                entryBuffer.Push(new SelectionEntry(_selectedEntities[i], 100 / _selectedEntities.Count, _selectedEntities[i].GetVirtualAllocation(this)));

            _entries.Clear();
            for (int i = 0; i < _selectedEntities.Count; i++) // make buffer the new source without ref change.
                _entries.Add(entryBuffer.Peek().Entity.Id, entryBuffer.Pop());

            IsPostAllocation = true;

            _totalVAllocation = _entries.Sum(e => e.Value.VAllocation);
            if (_totalVAllocation == 99) _totalVAllocation = 100;
        }

        public bool Add(string id)
        {
            if (Contains(id))
            {
                return false;
            }
            _selectedEntities.Add(_entities[id]);
            RecalculateAllocations();
            return true;
        }

        public bool Remove(string id)
        {
            if (!Contains(id))
            {
                return false;
            }
            _selectedEntities.Remove(_entities[id]);
            RecalculateAllocations();
            return true;
        }

        public void AddRange(params IEnumerable<string> ids)
        {
            foreach (string id in ids)
                if (!Add(id))
                    throw new ArgumentException($"Entity with id '{id}' already in selection.", nameof(ids));
        }

        public void RemoveRange(params IEnumerable<string> ids)
        {
            foreach (string id in ids)
                if (!Remove(id))
                    throw new ArgumentException($"Entity with id '{id}' not found.", nameof(ids));
        }

        public bool Contains(string id)
        {
            return Entries.ContainsKey(id);
        }

        public int GetSlot(string id) => _selectedEntities.GetFirstIndexWhere(e => e.Id == id);

        public int GetAllocation(string id) => _entries.TryGetValue(id, out SelectionEntry entry) ? entry.Allocation : 0;
        public int GetVirtualAllocation(string id) => IsPostAllocation ? (_entries.TryGetValue(id, out SelectionEntry entry) ? entry.VAllocation : 0) : throw new InvalidOperationException();

        public bool ContainsKey(string key) => Contains(key);
        public bool TryGetValue(string key, out SelectionEntry value) => _entries.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, SelectionEntry>> GetEnumerator() => _entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entries).GetEnumerator();
        IEnumerator<SelectionEntry> IEnumerable<SelectionEntry>.GetEnumerator() => _entries.Values.GetEnumerator();

        public override string ToString() => $"IsPostAllocation: {IsPostAllocation}, TotalVAllocation: {(IsPostAllocation ? TotalVAllocation : "<n/a>")}, Entries: [{string.Join(", ", Entries.Values.Select(e => e.ToString()))}]";
    }
}
