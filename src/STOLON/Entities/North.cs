namespace STOLON
{
    public class NorthEntity : Entity
    {
        public NorthEntity(ITexture2DCollection textures) : base("north", "North", "Nth", textures, fullName: "Noria-aeth")
        {
        }
        protected override EntityProfile ResolveProfile(ITexture2DCollection textures)
            => EntityProfile.GetDebug("north", textures);

        public override Computer? Computer => null;
    }
}
