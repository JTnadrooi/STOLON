namespace STOLON
{
    public class DeceitEntity : Entity
    {
        public DeceitEntity() : base("deceit", "Deceit", "Dc")
        {
        }

        protected override EntityProfile ResolveProfile()
            => new EntityProfile("deceit", null, new Point(-130, -40));

        public override Computer? Computer => null;
    }
}
