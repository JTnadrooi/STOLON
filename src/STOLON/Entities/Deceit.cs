namespace STOLON
{
    public class DeceitEntity : Entity
    {
        public DeceitEntity(ITexture2DCollection textures) : base("deceit", "Deceit", "Dc", textures)
        {
        }

        protected override EntityProfile ResolveProfile(ITexture2DCollection textures)
            => new EntityProfile("deceit", textures, null, new Point(-130, -40));

        public override Computer? Computer => null;
    }
}
