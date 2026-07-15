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

        /// <summary>
        /// Represents the maxmimum amount of tiles a token can travel.
        /// </summary>
        public const int MaxMovePathLength = 128;

        public GravityAffectedMove(int originX, int originY)
        {
            Origin = new Point(originX, originY);
        }

        public void Apply(BoardState state, Entity performer)
        {
            Span<Point> pathBuffer = stackalloc Point[MaxMovePathLength];
            int pathLength = GetPath(pathBuffer, state);
            Point finalPos = pathBuffer[pathLength - 1];

            TileAttributes attributes = TileAttribute.GetOccupiedTileAttributeFor(state.GetEntityIndex(performer));
            attributes |= TileAttributes.Solid;

            state.AddAttributes(finalPos, attributes);
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

        /// <summary>
        /// Gets if the token placed by this move passes through the specified <paramref name="pos"/>.
        /// </summary>
        public bool PassesThrough(Point pos, BoardState state)
        {
            Span<Point> buffer = stackalloc Point[MaxMovePathLength];
            int count = GetPath(buffer, state);
            for (int i = 0; i < count; i++)
                if (buffer[i] == pos)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns a <see cref="GravityAffectedMove"/> with the same eventual placement position, but with an origin that maximises the amount of tiles the token passes through.
        /// </summary>
        public GravityAffectedMove GetNormalized(BoardState state)
        {
            Tile originTile = state.Tiles[Origin.X, Origin.Y];

            bool hasGravDown = originTile.HasAttribute(TileAttributes.GravDown);
            bool hasGravUp = originTile.HasAttribute(TileAttributes.GravUp);

            if (!hasGravDown && !hasGravUp)
                return this;

            // determine extension direction (opposite of movement direction)
            int stepY;
            TileAttributes requiredAttr;
            if (hasGravDown)
            {
                stepY = 1;
                requiredAttr = TileAttributes.GravDown;
            }
            else
            {
                stepY = -1;
                requiredAttr = TileAttributes.GravUp;
            }

            Point newOrigin = Origin;
            while (true)
            {
                Point candidatePos = new Point(newOrigin.X, newOrigin.Y + stepY);

                if (!state.TryGetTileAt(candidatePos, out Tile? candidateTile))
                    break;

                // only extend through non‑solid tiles with the same gravity
                if (candidateTile.Value.IsSolid() || !candidateTile.Value.HasAttribute(requiredAttr))
                    break;

                newOrigin = candidatePos;
            }

            return new GravityAffectedMove(newOrigin.X, newOrigin.Y);
        }

        /// <summary>
        /// Fills a <see cref="Span{Point}"/> with the path taken by the token, starting from <see cref="Origin"/> until it stops.
        /// </summary>
        /// <param name="buffer">The span to fill with path points. Preferably longer than <see cref="MaxMovePathLength"/>.</param>
        /// <returns>The number of points written to the buffer.</returns>
        public int GetPath(Span<Point> buffer, BoardState state)
        {
            int count = 0;
            Point current = Origin;

            while (state.TryGetTileAt(current, out Tile? tile))
            {
                if (count >= buffer.Length)
                    throw new ArgumentException("Provided buffer is too small.", nameof(buffer));

                buffer[count++] = current;

                Point next;
                if (tile.Value.HasAttribute(TileAttributes.GravDown))
                    next = new Point(current.X, current.Y - 1);
                else if (tile.Value.HasAttribute(TileAttributes.GravUp))
                    next = new Point(current.X, current.Y + 1);
                else
                    break; // no gravity, movement stops

                // stop if the next tile is out of bounds or solid
                if (!state.TryGetTileAt(next, out Tile? nextTile) || nextTile.Value.IsSolid())
                    break;

                current = next;
            }

            return count;
        }

        /// <summary>
        /// Creates an array of all points the token passes through, from <see cref="Origin"/> to its final resting place. See also; <see cref="GetPath(Span{Microsoft.Xna.Framework.Point}, BoardState)"/>.
        /// </summary>
        public Point[] GetPath(BoardState state)
        {
            Span<Point> buffer = stackalloc Point[MaxMovePathLength];
            int count = GetPath(buffer, state);
            return buffer.Slice(0, count).ToArray();
        }

        public bool Equals(IMove? other)
        {
            return other is GravityAffectedMove otherMove && otherMove.Origin == Origin;
        }

        public override string ToString()
        {
            return Origin.ToString();
        }

        public static IMove[] GetUniqueMoves(BoardState state)
        {
            HashSet<Point> seenOrigins = new HashSet<Point>();
            List<GravityAffectedMove> moves = new List<GravityAffectedMove>();

            for (int x = 0; x < state.Dimensions.X; x++)
            {
                for (int y = 0; y < state.Dimensions.Y; y++)
                {
                    Point origin = new Point(x, y);
                    if (!state.TryGetTileAt(origin, out Tile? tile) || tile.Value.IsSolid() || tile.Value.HasAttribute(TileAttributes.Disabled))
                        continue;

                    GravityAffectedMove move = new GravityAffectedMove(origin.X, origin.Y);
                    GravityAffectedMove normalized = move.GetNormalized(state);
                    if (seenOrigins.Add(normalized.Origin))
                        moves.Add(normalized);
                }
            }

            return moves.ToArray();
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
                        Span<Point> path = gravMove.GetPath(state);
                        foreach (Point pos in path)
                        {
                            if (state.TryGetTileAt(pos, out Tile? tile))
                            {
                                if (tile.Value.GetHitbox().Contains(worldMousePos))
                                {
                                    if (_input.Mouse.IsClicked(MouseButton.Left))
                                    {
                                        pickedMove = move;
                                        return true;
                                    }
                                    break;
                                }
                            }
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
