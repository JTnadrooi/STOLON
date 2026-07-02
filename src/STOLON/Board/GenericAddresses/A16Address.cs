using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class A16Address : Address
    {
        public A16Address() : base("a16")
        {
        }

        public override BoardState GetInitialState(Entity player1, Entity player2)
        {
            return BoardState.GetDefault(player1, player2);
        }
    }
}
