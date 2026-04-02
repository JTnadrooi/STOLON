using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public class NorthEntity : Entity
    {
        public NorthEntity(ITexture2DCollection textures) : base("north", "North", "Nth", textures, fullName: "Noria-aeth")
        {
        }

        protected override EntityProfile ResolveProfile(ITexture2DCollection textures)
            => EntityProfile.GetDebug("north", textures);

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
