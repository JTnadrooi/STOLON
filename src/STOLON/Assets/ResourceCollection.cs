using AsitLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public abstract class ResourceCollection : IDisposable
    {
        private bool _disposedValue;

        public abstract void LoadResources();
        public abstract void UnloadResources();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    UnloadResources();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static TResourceCollection Load<TResourceCollection>() where TResourceCollection : ResourceCollection, new()
        {
            TResourceCollection collection = new TResourceCollection();
            collection.LoadResources();
            return collection;
        }
    }

    public abstract class ResourceCollection<TContent> : ResourceCollection, IEnumerable<TContent>, IReadOnlyDictionary<string, TContent>
    {
        public FrozenDictionary<string, TContent>? _resources;

        public FrozenDictionary<string, TContent> Resources
        {
            get => _resources ?? throw new InvalidOperationException($"Resources for ResourceCollection<{typeof(TContent)}> are not loaded yet.");
            private set => _resources = value;
        }

        public IEnumerable<string> Keys => Resources.Keys;
        public IEnumerable<TContent> Values => Resources.Values;
        public int Count => Resources.Count;
        public TContent this[string key] => Resources[key];

        private readonly IResourceLoader<TContent> _loader;

        public ResourceCollection(IResourceLoader<TContent> loader)
        {
            _loader = loader;
        }

        public virtual TContent GetReference(string path) => this[path];

        public bool ContainsKey(string key) => Resources.ContainsKey(key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TContent value) => Resources.TryGetValue(key, out value);

        public override void LoadResources() => Resources = _loader.GetResources();

        public override void UnloadResources()
        {
            if (Count == 0) return;
            foreach (TContent item in Resources.Values)
                if (item is IDisposable disposable)
                    disposable.Dispose();
        }

        IEnumerator<TContent> IEnumerable<TContent>.GetEnumerator() => Values.GetEnumerator();
        public IEnumerator GetEnumerator() => ((IEnumerable)Values).GetEnumerator();
        IEnumerator<KeyValuePair<string, TContent>> IEnumerable<KeyValuePair<string, TContent>>.GetEnumerator() => ((IEnumerable<KeyValuePair<string, TContent>>)Resources).GetEnumerator();
    }
}
