namespace STOLON
{
    public sealed class InitAddressCommandFlag : CommandFlag
    {
        public EntitySelection Selection { get; }

        public string Address { get; }

        public InitAddressCommandFlag(Entity[] entities, string address)
        {
            Selection = new EntitySelection(entities, 4);
            Address = address;
        }
    }
}
