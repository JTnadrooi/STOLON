using AsitLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
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

    public abstract class SequentialResourceLoader<TContent> : IResourceLoader<TContent>
    {
        public abstract TContent LoadFile(string file);
        public abstract string[] GetFiles();
        public abstract string GetId(string file);

        private readonly bool _multicore;
        private readonly Dictionary<string, TContent> _resources;

        public SequentialResourceLoader(bool multicore = true)
        {
            _multicore = multicore;
            _resources = new Dictionary<string, TContent>();
        }

        public FrozenDictionary<string, TContent> GetResources()
        {
            STOLON.Debug.Log($">[s]loading data of type '{typeof(TContent).Name}' sequentially.");

            foreach (string file in GetFiles())
            {
                STOLON.Debug.Log("found file: " + file);
                string toLoadId = GetId(file);
                STOLON.Debug.Log(">attempting load of resource with id/key: " + file);
                TContent loaderResult = LoadFile(file);
                _resources[toLoadId] = loaderResult;
                STOLON.Debug.Log("added with id: " + toLoadId);
                STOLON.Debug.Success();
            }

            STOLON.Debug.Log(_resources.Values.Count + " assets loaded.");
            STOLON.Debug.Success();

            return _resources.ToFrozenDictionary();
        }
    }

    public abstract class ResourceCollection<TContent> : IEnumerable<TContent>, IDisposable
    {
        public FrozenDictionary<string, TContent>? Resources { get; private set; }
        private bool _disposedValue;

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

        public virtual void LoadResources() => Resources = _loader.GetResources();

        public virtual void UnloadResources()
        {
            if (Count == 0) return;
            foreach (TContent item in Resources.Values)
                if (item is IDisposable disposable)
                    disposable.Dispose();
            //Resources.();
        }

        IEnumerator<TContent> IEnumerable<TContent>.GetEnumerator() => Values.GetEnumerator();
        public IEnumerator GetEnumerator() => ((IEnumerable)Values).GetEnumerator();

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

        public static TResourceCollection LoadCollection<TResourceCollection>() where TResourceCollection : ResourceCollection<TContent>, new()
        {
            TResourceCollection collection = new TResourceCollection();
            collection.LoadResources();
            return collection;
        }
    }
}
