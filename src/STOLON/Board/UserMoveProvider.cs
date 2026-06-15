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
            HashSet<TileAttribute> attributes = new HashSet<TileAttribute>();

            switch (state.GetEntityIndex(performer))
            {
                case 0:
                    attributes.Add(new Player0OccupiedTileAttribute());
                    break;
                case 1:
                    attributes.Add(new Player1OccupiedTileAttribute());
                    break;
            }
            attributes.Add(new SolidTileAttribute());

            Point alterPos = _position;
            while (true)
            {
                Tile alterTile = state.Tiles[alterPos.X, alterPos.Y];
                if (alterTile.HasAttribute<GravDownTileAttribute>())
                {
                    if (alterPos.Y - 1 >= 0)
                    {
                        Tile nextTile = state.Tiles[alterPos.X, alterPos.Y - 1];
                        if (!nextTile.IsSolid())
                            alterPos = new Point(alterPos.X, alterPos.Y - 1);
                        else break;
                    }
                    else break;
                }
                else if (alterTile.HasAttribute<GravUpTileAttribute>())
                {
                    if (alterPos.Y + 1 <= state.Dimensions.Y - 1)
                    {
                        Tile nextTile = state.Tiles[alterPos.X, alterPos.Y + 1];
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
                        Tile tile = state.Tiles[x, y];
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
