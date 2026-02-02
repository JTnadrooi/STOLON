namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public class CachedAudioResourceCollection : ResourceCollection<CachedAudio>, ICachedAudioResourceCollection
    {
        public CachedAudioResourceCollection(IResourceLoader<CachedAudio> loader) : base(loader) { }
    }
}
