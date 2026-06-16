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
        private readonly EntityDefinition _siloDefinition;

        public A16Address(ITexture2DCollection textures, IInputManager input, Kernel kernel, EntityDefinition[] entityDefinitions) : base("a16")
        {
            _input = input;
            _textures = textures;
            _kernel = kernel;
            _siloDefinition = entityDefinitions.First(d => d.Id == "silo");
        }

        public override BoardState GetInitialState()
        {
            return BoardState.GetDefault([new SiloEntity(_siloDefinition, _textures, new UserMoveProvider(_input, _kernel)), new SiloEntity(_siloDefinition, _textures, new UserMoveProvider(_input, _kernel))]);
        }
    }
}
