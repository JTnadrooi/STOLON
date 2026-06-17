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

        public void Apply(BoardState state, Entity performer)
        {
            TileAttributes attributes = TileAttributes.None;

            attributes |= TileAttribute.GetOccupiedTileAttributeFor(state.GetEntityIndex(performer));
            attributes |= TileAttributes.Solid;

            Point alterPos = _position;
            while (true)
            {
                ref Tile alterTile = ref state.Tiles[alterPos.X, alterPos.Y];
                if (alterTile.HasAttribute(TileAttributes.GravDown))
                {
                    if (alterPos.Y - 1 >= 0)
                    {
                        ref Tile nextTile = ref state.Tiles[alterPos.X, alterPos.Y - 1];
                        if (!nextTile.IsSolid())
                            alterPos = new Point(alterPos.X, alterPos.Y - 1);
                        else break;
                    }
                    else break;
                }
                else if (alterTile.HasAttribute(TileAttributes.GravUp))
                {
                    if (alterPos.Y + 1 <= state.Dimensions.Y - 1)
                    {
                        ref Tile nextTile = ref state.Tiles[alterPos.X, alterPos.Y + 1];
                        if (!nextTile.IsSolid())
                            alterPos = new Point(alterPos.X, alterPos.Y + 1);
                        else break;
                    }
                    else break;
                }
                else break;
            }

            state.Alter(alterPos, attributes);
            state.GoNextPlayer();
            state.RegisterMove(this, performer);
        }

        public void Undo(BoardState state, Entity performer)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class UserMoveProvider : IMoveProvider
    {
        private readonly IInputManager _input;
        private readonly Kernel _kernel;

        public bool IsComputer { get; }

        public UserMoveProvider(IInputManager input, Kernel kernel)
        {
            _input = input;
            _kernel = kernel;
        }

        public bool TryGetMove(BoardState state, ReadOnlySpan<IMove> availableMoves, [NotNullWhen(true)] out IMove? bestMove)
        {
            BoardWindow window = (BoardWindow)_kernel.Windows.First(w => w.GetType() == typeof(BoardWindow));
            Board board = window.Board;
            Vector2 worldMousePos = board.Camera.Unproject(_input.Mouse.Position - window.Position);

            if (_input.Mouse.IsClicked(MouseButton.Left))
            //if (_input.Mouse.IsClicked(MouseButton.Left) && _input.IsMouseOn<Board>())
            {
                for (int x = 0; x < state.Tiles.GetLength(0); x++)
                    for (int y = 0; y < state.Tiles.GetLength(1); y++)
                    {
                        ref Tile tile = ref state.Tiles[x, y];
                        if (tile.GetHitbox().Contains(worldMousePos) && !tile.IsSolid())
                        {
                            bestMove = new GravityAffectedMove(x, y);
                            return true;
                        }
                    }
            }

            bestMove = null;
            return false;
        }
    }
}
