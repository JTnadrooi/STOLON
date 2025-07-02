using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _inner;
        private readonly Func<TKey, TValue> _defaultValueFactory;
        private bool _caching;

        public DefaultDictionary(Func<TKey, TValue> defaultValueFactory, bool caching = false)
        {
            _inner = new();
            _defaultValueFactory = defaultValueFactory;
            _caching = caching;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!_inner.TryGetValue(key, out var value))
                {
                    value = _defaultValueFactory(key);
                    if (_caching) _inner[key] = value;
                }
                return value;
            }
            set => _inner[key] = value;
        }

        public ICollection<TKey> Keys => _inner.Keys;
        public ICollection<TValue> Values => _inner.Values;
        public int Count => _inner.Count;
        public bool IsReadOnly => false;
        public void Add(TKey key, TValue value) => _inner.Add(key, value);
        public bool ContainsKey(TKey key) => _inner.ContainsKey(key);
        public bool Remove(TKey key) => _inner.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => _inner.TryGetValue(key, out value);
        public void Add(KeyValuePair<TKey, TValue> item) => _inner.Add(item.Key, item.Value);
        public void Clear() => _inner.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => _inner.Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item) => _inner.Remove(item.Key);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    }

    //public class UIElementUpdateDump
    //{
    //    private readonly Dictionary<string, UIElementUpdateData> _updateDump;

    //    public UIElementUpdateDump(int capacity)
    //    {
    //        _updateDump = new Dictionary<string, UIElementUpdateData>(capacity);
    //    }
    //}
}
