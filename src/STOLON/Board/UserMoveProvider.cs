using MonoGame.Extended;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public sealed class ColumnDisableMove : IMove
    {
        public readonly int ColumnIndex;

        public ColumnDisableMove(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }

        public void Apply(BoardState state, Entity performer)
        {
            for (int i = 0; i < state.Dimensions.Y; i++)
            {
                state.AddAttributes(new Point(ColumnIndex, i), TileAttributes.Disabled1);
            }

            if (performer is SiloEntity silo)
            {
                silo.LastAbilityUse = state.CurrentMoveIndex;
            }

            state.RegisterMove(this, performer);

            Console.WriteLine(this);
        }

        public void Undo(BoardState state, Entity performer)
        {
            throw new NotImplementedException();
        }

        public string GetCommand()
        {
            return "s@ " + ColumnIndex;
        }

        public bool Equals(IMove? other)
        {
            return other is ColumnDisableMove otherMove && otherMove.ColumnIndex == ColumnIndex;
        }

        public override string ToString()
        {
            return ColumnIndex.ToString();
        }
    }

    public sealed class GravityAffectedMove : IMove
    {
        public readonly Point Origin;

        public GravityAffectedMove(int originX, int originY)
        {
            Origin = new Point(originX, originY);
        }

        public void Apply(BoardState state, Entity performer)
        {
            TileAttributes attributes = TileAttributes.None;

            attributes |= TileAttribute.GetOccupiedTileAttributeFor(state.GetEntityIndex(performer));
            attributes |= TileAttributes.Solid;

            Point alterPos = Origin;
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

        public string GetCommand()
        {
            return "@ " + Board.GetCoordsFromPoint(Origin);
        }

        public bool Equals(IMove? other)
        {
            return other is GravityAffectedMove otherMove && otherMove.Origin == Origin;
        }

        public override string ToString()
        {
            return Origin.ToString();
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
            BoardWindow window = (BoardWindow)_kernel.Windows.First(w => w is BoardWindow);
            Board board = window.Board;
            Vector2 windowMousePos = _input.Mouse.Position - window.Position;
            Vector2 worldMousePos = board.Camera.Unproject(windowMousePos);

            foreach (IMove move in availableMoves)
            {
                switch (move)
                {
                    case GravityAffectedMove gravMove:
                        Tile? tile;
                        Point currentPos = gravMove.Origin;
                        while (true)
                        {
                            if (state.TryGetTileAt(currentPos, out tile))
                            {
                                if (tile.Value.IsSolid())
                                {
                                    break;
                                }
                                if (_input.Mouse.IsClicked(MouseButton.Left) && tile.Value.GetHitbox().Contains(worldMousePos))
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
                    case ColumnDisableMove columnDisableMove:
                        for (int y = 0; y < state.Dimensions.Y; y++)
                        {
                            if (_input.Mouse.IsClicked(MouseButton.Right) && state.Tiles[columnDisableMove.ColumnIndex, y].GetHitbox().Contains(worldMousePos))
                            {
                                pickedMove = move;
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            pickedMove = null;
            return false;
        }
    }
}
