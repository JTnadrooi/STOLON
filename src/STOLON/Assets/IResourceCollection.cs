using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public interface IResourceCollection : IDisposable
    {
        bool IsLoaded { get; }
        void LoadResources();
        void UnloadResources();
    }

    public interface IResourceCollection<TContent> : IResourceCollection, IEnumerable<TContent>, IReadOnlyDictionary<string, TContent>
    {
        FrozenDictionary<string, TContent> Resources { get; }
        TContent GetReference(string path);
    }
}