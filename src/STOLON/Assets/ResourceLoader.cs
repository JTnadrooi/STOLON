using System.Collections.Concurrent;
using System.Collections.Frozen;

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
            STOLON.Logger.Log($">[s]loading data of type '{typeof(TContent).Name}' in parallel.");
            STOLON.Logger.Log($">getting items..");

            string[] items = GetItems();
            foreach (string item in items)
            {
                STOLON.Logger.Log("found item: " + item);
            }

            STOLON.Logger.Success();
            STOLON.Logger.Log($">loading resources.");

            Parallel.ForEach(items, item =>
            {
                string toLoadId = GetId(item);
                STOLON.Logger.LogThreadSafe($"loading resource from '{item}' as '{toLoadId}'.");
                TContent loaderResult = LoadItem(item);
                _resources[toLoadId] = loaderResult;
                STOLON.Logger.LogThreadSafe($"succesfully loaded resource from '{item}' as '{toLoadId}'.");
            });

            STOLON.Logger.Success();
            STOLON.Logger.Success(_resources.Values.Count + " assets loaded.");

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
            STOLON.Logger.Log($">[s]loading data of type '{typeof(TContent).Name}' in sequence.");
            STOLON.Logger.Log($">getting items..");

            string[] items = GetItems();
            foreach (string item in items)
            {
                STOLON.Logger.Log("found item: " + item);
            }

            STOLON.Logger.Success();
            STOLON.Logger.Log($">loading resources.");

            foreach (string item in items)
            {
                string toLoadId = GetId(item);
                STOLON.Logger.Log($">loading resource from '{item}' as '{toLoadId}'.");
                TContent loaderResult = LoadItem(item);
                _resources[toLoadId] = loaderResult;
                STOLON.Logger.Success();
            }
            STOLON.Logger.Success();

            STOLON.Logger.Success(_resources.Values.Count + " assets loaded.");

            return _resources.ToFrozenDictionary();
        }
    }
}
