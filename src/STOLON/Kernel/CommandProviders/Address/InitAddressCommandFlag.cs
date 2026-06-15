namespace STOLON
{
    public sealed class InitAddressCommandFlag : CommandFlag
    {
        public EntitySelection Selection { get; }

        public Address Address { get; }

        public InitAddressCommandFlag(EntityDefinition[] entities, Address address)
        {
            Selection = new EntitySelection(entities, 4);
            Address = address;
        }
    }
}
