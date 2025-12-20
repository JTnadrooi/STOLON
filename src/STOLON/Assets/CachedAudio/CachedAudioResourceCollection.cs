namespace STOLON
{
    public class CachedAudioResourceCollection : ResourceCollection<CachedAudio>
    {
        public CachedAudioResourceCollection() : base(new CachedAudioResourceLoader()) { }
    }
}
