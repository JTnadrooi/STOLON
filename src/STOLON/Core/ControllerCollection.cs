using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public interface IController : IUpdatable { }

    public class ControllerCollection : IDictionary<string, IController>, IUpdatable
    {
        private readonly Dictionary<string, IController> _controllers; // does not preserve order.

        public ControllerCollection()
        {
            _controllers = new Dictionary<string, IController>();
        }

        public IController this[string key]
        {
            get => _controllers[key];
            set => _controllers[key] = value;
        }

        public ICollection<string> Keys => _controllers.Keys;
        public ICollection<IController> Values => _controllers.Values;
        public int Count => _controllers.Count;
        public bool IsReadOnly => false;

        public void Add(string key, IController value) => _controllers.Add(key, value);
        public void Add(KeyValuePair<string, IController> item) => ((ICollection<KeyValuePair<string, IController>>)_controllers).Add(item);
        public void Clear() => _controllers.Clear();
        public bool Contains(KeyValuePair<string, IController> item) => _controllers.Contains(item);
        public bool ContainsKey(string key) => _controllers.ContainsKey(key);
        public void CopyTo(KeyValuePair<string, IController>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, IController>>)_controllers).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<string, IController>> GetEnumerator() => _controllers.GetEnumerator();

        public bool Remove(string key) => _controllers.Remove(key);
        public bool Remove(KeyValuePair<string, IController> item) => ((ICollection<KeyValuePair<string, IController>>)_controllers).Remove(item);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IController value) => _controllers.TryGetValue(key, out value);

        public void Update(int elapsedMilliseconds)
        {
            foreach ((_, IController controller) in _controllers)
            {
                controller.Update(elapsedMilliseconds);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => _controllers.GetEnumerator();
    }
}
