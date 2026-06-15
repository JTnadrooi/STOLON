using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public interface IMoveProvider
    {
        // called every frame for current turn IPlayer
        // might remove gameInfo param/type
        bool TryGetMove(BoardState state, ReadOnlySpan<IMove> availableMoves, [NotNullWhen(true)] out IMove? bestMove);
    }
}
