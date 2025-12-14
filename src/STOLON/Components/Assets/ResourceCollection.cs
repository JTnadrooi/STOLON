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
    public interface IResourceLoader<TContent>
    {
        public FrozenDictionary<string, TContent> GetResources();
    }

    public abstract class ParallelResourceLoader<TContent> : IResourceLoader<TContent>
    {
        public abstract TContent LoadItem(string item);
        public abstract string[] GetItems();
        public abstract string GetId(string item);

        private readonly bool _multicore;
        private readonly ConcurrentDictionary<string, TContent> _resources;

        public ParallelResourceLoader(bool multicore = true)
        {
            _multicore = multicore;
            _resources = new ConcurrentDictionary<string, TContent>();
        }

        public FrozenDictionary<string, TContent> GetResources()
        {
            STOLON.Debug.Log($">[s]loading data of type '{typeof(TContent).Name}' in parallel.");
            STOLON.Debug.Log($">getting items..");

            string[] items = GetItems();
            foreach (string item in items)
            {
                STOLON.Debug.Log("found item: " + item);
            }

            STOLON.Debug.Success();
            STOLON.Debug.Log($">loading resources.");

            Parallel.ForEach(items, item =>
            {
                string toLoadId = GetId(item);
                STOLON.Debug.LogThreadSafe($"loading resource from '{item}' as '{toLoadId}'.");
                TContent loaderResult = LoadItem(item);
                _resources[toLoadId] = loaderResult;
                STOLON.Debug.LogThreadSafe($"succesfully loaded resource from '{item}' as '{toLoadId}'.");
            });

            STOLON.Debug.Success();
            STOLON.Debug.Success(_resources.Values.Count + " assets loaded.");

            return _resources.ToFrozenDictionary();
        }
    }

    public abstract class SequentialResourceLoader<TContent> : IResourceLoader<TContent>
    {
        public abstract TContent LoadItem(string item);
        public abstract string[] GetItems();
        public abstract string GetId(string item);

        private readonly bool _multicore;
        private readonly Dictionary<string, TContent> _resources;

        public SequentialResourceLoader(bool multicore = true)
        {
            _multicore = multicore;
            _resources = new Dictionary<string, TContent>();
        }

        public FrozenDictionary<string, TContent> GetResources()
        {
            STOLON.Debug.Log($">[s]loading data of type '{typeof(TContent).Name}' in sequence.");
            STOLON.Debug.Log($">getting items..");

            string[] items = GetItems();
            foreach (string item in items)
            {
                STOLON.Debug.Log("found item: " + item);
            }

            STOLON.Debug.Success();
            STOLON.Debug.Log($">loading resources.");

            foreach (string item in items)
            {
                string toLoadId = GetId(item);
                STOLON.Debug.Log($">loading resource from '{item}' as '{toLoadId}'.");
                TContent loaderResult = LoadItem(item);
                _resources[toLoadId] = loaderResult;
                STOLON.Debug.Success();
            }
            STOLON.Debug.Success();

            STOLON.Debug.Success(_resources.Values.Count + " assets loaded.");

            return _resources.ToFrozenDictionary();
        }
    }

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

        public static TResourceCollection LoadCollection<TResourceCollection>() where TResourceCollection : ResourceCollection, new()
        {
            TResourceCollection collection = new TResourceCollection();
            collection.LoadResources();
            return collection;
        }
    }

    public abstract class ResourceCollection<TContent> : ResourceCollection, IEnumerable<TContent>
    {
        public FrozenDictionary<string, TContent>? Resources { get; private set; }

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
            //Resources.();
        }

        IEnumerator<TContent> IEnumerable<TContent>.GetEnumerator() => Values.GetEnumerator();
        public IEnumerator GetEnumerator() => ((IEnumerable)Values).GetEnumerator();
    }
}
