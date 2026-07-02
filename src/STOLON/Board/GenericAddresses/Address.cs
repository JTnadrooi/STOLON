using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public abstract class Address
    {
        // something something map

        public string Id { get; }

        protected Address(string id)
        {
            Id = id;
        }

        public abstract BoardState GetInitialState(Entity player1, Entity player2);
    }
}
