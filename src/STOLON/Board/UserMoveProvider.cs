using MonoGame.Extended;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public sealed class GravityAffectedMove : IMove
    {
        private readonly Point _position;

        public GravityAffectedMove(int tileX, int tileY)
        {
            _position = new Point(tileX, tileY);
        }

        public void Apply(BoardState boardState, Entity performer)
        {
            HashSet<TileAttribute> attributes = new HashSet<TileAttribute>();

            switch (boardState.GetEntityIndex(performer))
            {
                case 0:
                    attributes.Add(new Player0OccupiedTileAttribute());
                    break;
                case 1:
                    attributes.Add(new Player1OccupiedTileAttribute());
                    break;
            }

            boardState.Alter(_position, attributes);
        }
    }

    public sealed class UserMoveProvider : IMoveProvider
    {
        private readonly IInputManager _input;
        private readonly Kernel _kernel;

        public ImmutableArray<SearchTarget> SearchTargets { get; }

        public bool IsComputer { get; }

        public UserMoveProvider(IInputManager input, Kernel kernel)
        {
            _input = input;
            _kernel = kernel;
            SearchTargets = SearchTarget.GetDefaultTargets();
        }

        public bool TryGetMove(BoardState state, IMove[] availableMoves, [NotNullWhen(true)] out IMove? bestMove)
        {
            Board board = ((BoardWindow)_kernel.Windows.First(w => w.GetType() == typeof(BoardWindow))).Board;
            Vector2 worldMousePos = board.Camera.Unproject(_input.Mouse.Position);

            if (_input.Mouse.IsClicked(MouseButton.Left))
            //if (_input.Mouse.IsClicked(MouseButton.Left) && _input.IsMouseOn<Board>())
            {
                //for (int x = 0; x < state.Tiles.GetLength(0); x++)
                //    for (int y = 0; y < state.Tiles.GetLength(1); y++)
                //        if (state.Tiles[x, y].GetHitbox().Contains(worldMousePos) && !state.Tiles[x, y].IsSolid())
                //        {
                //            bestMove = new GravityAffectedMove(x, y);
                //            return true;
                //        }
                bestMove = new GravityAffectedMove(0, 0);
                return true;
            }

            bestMove = null;
            return false;
        }
    }
}
