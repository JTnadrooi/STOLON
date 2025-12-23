namespace STOLON
{
    public class EffectResourceCollection : ResourceCollection<Effect>, IEffectResourceCollection, ISingletonDependency
    {
        public EffectResourceCollection(IResourceLoader<Effect> loader) : base(loader) { }
    }
}
