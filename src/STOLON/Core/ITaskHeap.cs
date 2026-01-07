
namespace STOLON
{
    public interface ITaskHeap : IUpdatable
    {
        ReadOnlyDictionary<string, object?> FrameCompletedTasks { get; }
        ReadOnlyDictionary<string, DynamicTask> Functions { get; }

        string EnsurePush(DynamicTask dynamicTask, int waitTime);
        object? ForceRun(string id);
        string[] GetHistory();
        bool IsCompleted(string id);
        bool IsCompleted<T>(string id, out T? returned);
        bool IsQueued(string id);
        void Push(string id, DynamicTask dynamicTask, int waitTime);
        void SafePush(string id, DynamicTask dynamicTask, int waitTime, bool overwrite = true);
    }
}