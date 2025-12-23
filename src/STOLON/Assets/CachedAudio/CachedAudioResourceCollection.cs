namespace STOLON
{
    public class CachedAudioResourceCollection : ResourceCollection<CachedAudio>, ICachedAudioResourceCollection, ISingletonDependency
    {
        public CachedAudioResourceCollection(IResourceLoader<CachedAudio> loader) : base(loader) { }
    }
}
