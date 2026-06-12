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
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public A16Address(ITexture2DCollection textures, IInputManager input, Kernel kernel) : base("a16")
        {
            _input = input;
            _textures = textures;
            _kernel = kernel;
        }

        public override BoardState GetInitialBoardState()
        {
            return BoardState.GetDefault([new SiloEntity(_textures, new UserMoveProvider(_input, _kernel)), new SiloEntity(_textures, new UserMoveProvider(_input, _kernel))]);
        }
    }
}
