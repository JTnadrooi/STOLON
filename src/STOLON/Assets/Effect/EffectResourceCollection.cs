namespace STOLON
{
    public class EffectResourceCollection : ResourceCollection<Effect>
    {
        public EffectResourceCollection() : base(new EffectResourceLoader()) { }
    }
}
