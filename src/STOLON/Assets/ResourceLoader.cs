using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
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
}
