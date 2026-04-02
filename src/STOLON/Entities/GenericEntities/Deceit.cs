using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public class DeceitEntity : Entity
    {
        public DeceitEntity(ITexture2DCollection textures) : base("deceit", "Deceit", "Dc", textures)
        {
        }

        protected override EntityProfile ResolveProfile(ITexture2DCollection textures)
            => new EntityProfile("deceit", textures, null, new Point(-130, -40));

        public override bool HasWon(BoardState state, GameInfo gameInfo)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetMove(BoardState state, GameInfo gameInfo, [NotNullWhen(true)] out Move? move)
        {
            throw new NotImplementedException();
        }
    }
}
