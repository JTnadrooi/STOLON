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

        private readonly IThreadSafeRichLogger _logger;

        public ParallelResourceLoader(IThreadSafeRichLogger logger, bool multicore = true)
        {
            _logger = logger;

            _multicore = multicore;
            _resources = new ConcurrentDictionary<string, TContent>();
        }

        public FrozenDictionary<string, TContent> GetResources()
        {
            _logger.Log($">[s]loading data of type '{typeof(TContent).Name}' in parallel.");
            _logger.Log($">getting items..");

            string[] items = GetItems();
            foreach (string item in items)
            {
                _logger.Log("found item: " + item);
            }

            _logger.Success();
            _logger.Log($">loading resources.");

            Parallel.ForEach(items, item =>
            {
                string toLoadId = GetId(item);
                _logger.LogThreadSafe($"loading resource from '{item}' as '{toLoadId}'.");
                TContent loaderResult = LoadItem(item);
                _resources[toLoadId] = loaderResult;
                _logger.LogThreadSafe($"succesfully loaded resource from '{item}' as '{toLoadId}'.");
            });

            _logger.Success();
            _logger.Success(_resources.Values.Count + " assets loaded.");

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

        private readonly IRichLogger _logger;

        public SequentialResourceLoader(IRichLogger logger, bool multicore = true)
        {
            _logger = logger;

            _multicore = multicore;
            _resources = new Dictionary<string, TContent>();
        }

        public FrozenDictionary<string, TContent> GetResources()
        {
            _logger.Log($">[s]loading data of type '{typeof(TContent).Name}' in sequence.");
            _logger.Log($">getting items..");

            string[] items = GetItems();
            foreach (string item in items)
            {
                _logger.Log("found item: " + item);
            }

            _logger.Success();
            _logger.Log($">loading resources.");

            foreach (string item in items)
            {
                string toLoadId = GetId(item);
                _logger.Log($">loading resource from '{item}' as '{toLoadId}'.");
                TContent loaderResult = LoadItem(item);
                _resources[toLoadId] = loaderResult;
                _logger.Success();
            }
            _logger.Success();

            _logger.Success(_resources.Values.Count + " assets loaded.");

            return _resources.ToFrozenDictionary();
        }
    }
}
