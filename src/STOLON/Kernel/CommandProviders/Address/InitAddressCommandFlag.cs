namespace STOLON
{
    public sealed class InitAddressCommandFlag : CommandFlag
    {
        //public EntitySelection Player1Selection { get; }
        //public EntitySelection Player2Selection { get; }
        public Entity? Player1 { get; set; }
        public Entity? Player2 { get; set; }
        public Address Address { get; }

        public InitAddressCommandFlag(Address address)
        {
            //Player1Selection = new EntitySelection(entities, 4);
            //Player2Selection = new EntitySelection(entities, 4);
            Address = address;
        }

        public Entity? GetPlayer(int playerindex)
        {
            if (playerindex == 0)
            {
                return Player1;
            }
            else if (playerindex == 1)
            {
                return Player2;
            }
            else throw new ArgumentOutOfRangeException(nameof(playerindex));
        }

        public void SetPlayer(int playerindex, Entity? entity)
        {
            if (playerindex == 0)
            {
                Player1 = entity;
            }
            else if (playerindex == 1)
            {
                Player2 = entity;
            }
            else throw new ArgumentOutOfRangeException(nameof(playerindex));
        }
    }
}
