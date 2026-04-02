using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class A16Address : Address
    {
        private readonly IInputManager _input;

        public A16Address(IInputManager input) : base("a16")
        {
            _input = input;
        }

        public override BoardState GetInitialBoardState()
        {
            return BoardState.GetDefault([new UserPlayer(_input, "user1"), new UserPlayer(_input, "user2")]);
        }
    }
}
