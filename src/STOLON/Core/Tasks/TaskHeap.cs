namespace STOLON
{
    /// <summary>
    /// Provides a way to "fire and forget" simple game logic.
    /// </summary>
    [Dependency(ServiceLifetime.Singleton)]
    public sealed class TaskHeap : ITaskHeap
    {
        public ReadOnlyDictionary<string, DynamicTask> Functions { get; }
        public ReadOnlyDictionary<string, object?> FrameCompletedTasks { get; }

        private Dictionary<string, DynamicTask> _taskDictionary;
        private Dictionary<string, object?> _frameCompletedTasks;
        private Dictionary<string, int> _taskWaitDataCollection;
        private List<string> _allCompletedTasks;

        private readonly IRichLogger _logger;

        public TaskHeap(IRichLogger logger)
        {
            _logger = logger;

            _taskDictionary = new Dictionary<string, DynamicTask>();
            _frameCompletedTasks = new Dictionary<string, object?>();
            _taskWaitDataCollection = new Dictionary<string, int>();
            _allCompletedTasks = new List<string>();
            Functions = _taskDictionary.AsReadOnly();
            FrameCompletedTasks = _frameCompletedTasks.AsReadOnly();
        }

        public void Update(int elapsedMilliseconds)
        {
            _frameCompletedTasks.Clear();

            foreach (KeyValuePair<string, DynamicTask> taskKvp in _taskDictionary)
            {
                if (_taskWaitDataCollection[taskKvp.Key] < 0)
                {
                    _logger.Log("(interupt:taskheap) runningtask with id; " + taskKvp.Key);
                    ForceRun(taskKvp.Key);
                }
                else _taskWaitDataCollection[taskKvp.Key] -= elapsedMilliseconds;
            }
        }

        public object? ForceRun(string id)
        {
            object? ret = _taskDictionary[id].Run();

            _frameCompletedTasks.Add(id, ret);
            _allCompletedTasks.Add(id);

            _taskWaitDataCollection.Remove(id);
            _taskDictionary.Remove(id);

            return ret;
        }

        public string[] GetHistory() => _allCompletedTasks.ToArray();
        public bool IsCompleted(string id) => _frameCompletedTasks.ContainsKey(id);
        public bool IsCompleted<T>(string id, out T? returned)
        {
            if (IsCompleted(id))
            {
                returned = (T?)(FrameCompletedTasks[id]);
                return true;
            }
            else
            {
                returned = default(T?); // null does not work for reasons unknown.
                return false;
            }
        }

        public bool IsQueued(string id) => _taskDictionary.ContainsKey(id);

        /// <summary>
        /// Push a task to the <see cref="TaskHeap"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="Task"/> on the <see cref="TaskHeap"/>.</param>
        /// <param name="dynamicTask">The <see cref="Task"/> to push.</param>
        /// <param name="waitTime">The time to wait before starting the task.</param>
        /// <param name="overwrite">If the task.</param>
        public void SafePush(string id, DynamicTask dynamicTask, int waitTime, bool overwrite = true)
        {

            if (waitTime < 0)
            {
                object? ret = dynamicTask.Run();
                _logger.Log("insta-ran task with id: " + id);
                _frameCompletedTasks.Add(id, ret);
                _allCompletedTasks.Add(id);
                return;
            }

            if (_taskDictionary.ContainsKey(id))
                if (overwrite) _logger.Log("key already known, overwriting task with id: " + id);
                else return;
            _taskWaitDataCollection[id] = waitTime;
            _taskDictionary[id] = dynamicTask;
            _logger.Log("pushed task with id: " + id);
        }

        public void Push(string id, DynamicTask dynamicTask, int waitTime)
        {
            if (IsQueued(id)) throw new Exception();
            else SafePush(id, dynamicTask, waitTime);
        }

        public string EnsurePush(DynamicTask dynamicTask, int waitTime)
        {
            _logger.Log(">ensuring task push.");
            string id = Enumerable.Range(0, int.MaxValue).Select(i => "__" + i).First(key => !_taskDictionary.ContainsKey(key));

            Push(id, dynamicTask, waitTime);

            _logger.Log("task pushed with id: " + id);
            _logger.Success();

            return id;
        }

    }
}
