using MonoGame.Extended;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public sealed class GravityAffectedMove : IMove
    {
        public readonly Point Position;

        public GravityAffectedMove(int tileX, int tileY)
        {
            Position = new Point(tileX, tileY);
        }

        public void Apply(BoardState state, Entity performer)
        {
            TileAttributes attributes = TileAttributes.None;

            attributes |= TileAttribute.GetOccupiedTileAttributeFor(state.GetEntityIndex(performer));
            attributes |= TileAttributes.Solid;

            Point alterPos = Position;
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
            attributes |= state.Tiles[alterPos.X, alterPos.Y].Attributes;

            state.Alter(alterPos, attributes);
            state.GoNextPlayer();
            state.RegisterMove(this, performer);
        }

        public void Undo(BoardState state, Entity performer)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IMove? other)
        {
            return other is GravityAffectedMove gravityAffectedMove && gravityAffectedMove.Position == Position;
        }

        public override string ToString()
        {
            return Position.ToString();
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

        public bool TryGetMove(BoardState state, ReadOnlySpan<IMove> availableMoves, [NotNullWhen(true)] out IMove? pickedMove)
        {
            BoardWindow window = (BoardWindow)_kernel.Windows.First(w => w.GetType() == typeof(BoardWindow));
            Board board = window.Board;
            Vector2 windowMousePos = _input.Mouse.Position - window.Position;
            Vector2 worldMousePos = board.Camera.Unproject(windowMousePos);

            if (_input.Mouse.IsClicked(MouseButton.Left))
            //if (_input.Mouse.IsClicked(MouseButton.Left) && _input.IsMouseOn<Board>())
            {
                foreach (IMove move in availableMoves)
                {
                    switch (move)
                    {
                        case GravityAffectedMove gravMove:
                            Tile? tile;
                            Point currentPos = gravMove.Position;
                            while (true)
                            {
                                if (state.TryGetTileAt(currentPos, out tile))
                                {
                                    if (tile.Value.IsSolid())
                                    {
                                        break;
                                    }
                                    if (tile.Value.GetHitbox().Contains(worldMousePos))
                                    {
                                        pickedMove = move;
                                        return true;
                                    }
                                    if (tile.Value.HasAttribute(TileAttributes.GravDown))
                                    {
                                        currentPos = new Point(currentPos.X, currentPos.Y - 1);
                                    }
                                    else if (tile.Value.HasAttribute(TileAttributes.GravUp))
                                    {
                                        currentPos = new Point(currentPos.X, currentPos.Y + 1);
                                    }
                                }
                                else break;
                            }
                            break;
                        default:
                            break;
                    }
                }

            }

            pickedMove = null;
            return false;
        }
    }
}
