namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public class EffectResourceCollection : ResourceCollection<Effect>, IEffectResourceCollection
    {
        public EffectResourceCollection(IResourceLoader<Effect> loader) : base(loader) { }
    }
}
