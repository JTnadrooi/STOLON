namespace STOLON
{
    public class NorthEntity : Entity
    {
        public NorthEntity() : base("north", "North", "Nth", fullName: "Noria-aeth")
        {
        }
        protected override EntityProfile ResolveProfile()
            => EntityProfile.GetDebug("north");

        public override Computer? Computer => null;
    }
}
