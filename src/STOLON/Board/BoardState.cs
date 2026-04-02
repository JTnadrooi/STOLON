using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public sealed class BoardState
    {
        public Point Dimensions => _dimensions;
        public Tile[,] Tiles => _tiles;
        public IPlayer[] Players => _players;
        public IPlayer CurrentPlayer => Players[_currentPlayerIndex];

        public readonly Stack<UndoObj> _undoStack;
        public readonly Collection<UndoObj> _undoSet;
        private readonly FrozenDictionary<IPlayer, int>? _playerIndexCache;
        private readonly Tile[,] _tiles;
        private readonly IPlayer[] _players;
        private readonly Point _dimensions;

        public int _currentPlayerIndex;

        public BoardState(Tile[,] tiles, IPlayer[] players, int currentPlayer = 0)
        {
            _tiles = tiles;
            _players = players;
            _dimensions = new Point(tiles.GetLength(0), tiles.GetLength(1));
            _undoStack = new Stack<UndoObj>();
            _undoSet = new Collection<UndoObj>();
            _playerIndexCache = players.Select((p, i) => new KeyValuePair<IPlayer, int>(p, i)).ToFrozenDictionary();

            _currentPlayerIndex = currentPlayer;
        }

        public void GoNextPlayer()
        {
            _currentPlayerIndex = _currentPlayerIndex == 0 ? 1 : 0;  // 2 player support only for now...
        }

        public BoardState DeepCopy() // for multithread magic
        {
            Tile[,] resultTiles = new Tile[_tiles.GetLength(0), _tiles.GetLength(1)];

            for (int x = 0; x < _dimensions.X; x++)
                for (int y = 0; y < _dimensions.Y; y++)
                    resultTiles[x, y] = _tiles[x, y].Clone();

            BoardState result = new BoardState(resultTiles, _players, _currentPlayerIndex);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tile GetTileAt(Point p) => _tiles[p.X, p.Y];

        public bool TryGetTileAt(Point p, [NotNullWhen(true)] out Tile? tile)
        {
            if (p.X >= 0 && p.X < _tiles.GetLength(0) &&
                p.Y >= 0 && p.Y < _tiles.GetLength(1))
            {
                tile = _tiles[p.X, p.Y];
                return true;
            }

            tile = default;
            return false;
        }

        public bool SearchFor(ReadOnlySpan<SearchTarget> searchTargets)
        {
            foreach (SearchTarget target in searchTargets)
                if (SearchFor(target)) return true;
            return false;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)] // maybe remove if method becomes too big. (method became too big)
        public bool SearchFor(in SearchTarget target)
        {
            for (int x = 0; x < _dimensions.X; x++)
            {
                for (int y = 0; y < _dimensions.Y; y++)
                {
                    int occupiedPlayerId = _tiles[x, y].GetOccupiedByPlayerIndex();

                    if (occupiedPlayerId == -1) continue;

                    int score = 0;

                    for (int i = 0; i < target.Nodes.Length; i++)
                    {
                        if (TryGetTileAt(new Point(target.Nodes[i].X + x, target.Nodes[i].Y + y), out Tile? tile))
                        {
                            if (tile.GetOccupiedByPlayerIndex() == occupiedPlayerId) score++;
                        }
                    }

                    if (score == target.Nodes.Length) return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(UserPlayer player) => _playerIndexCache[player];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Alter(Point tilePos, HashSet<TileAttribute> newAttributes) // SO SLOWWWW
        {
            Tiles[tilePos.X, tilePos.Y].Attributes = new HashSet<TileAttribute>(newAttributes);
            return true;
        }

        public BoardPreview GetPreview(Font2D font)
            => new BoardPreview(this, font);

        public void Undo()
        {
            UndoObj undoObj = _undoStack.Pop();
            _undoSet.Remove(undoObj);

            if (undoObj.NextPlayer) _currentPlayerIndex = _currentPlayerIndex == 0 ? 1 : 0;

            undoObj.Sim.Attributes.Remove((TileAttribute)TileAttribute.Attributes["Player" + _currentPlayerIndex + "Occupied"]);
            undoObj.Sim.Attributes.Remove(TileAttribute.Get<SolidTileAttribute>());

            Alter(undoObj.Sim.Position, undoObj.Sim.Attributes);
        }

        public static BoardState GetDefault(IPlayer[] players)
            => new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players);

        public readonly struct UndoObj
        {
            public Tile Sim { get; }

            public bool NextPlayer { get; }

            public UndoObj(Tile sim, bool nextPlayer)
            {
                Sim = sim;
                NextPlayer = nextPlayer;
            }

            public override int GetHashCode()
            {
                return Sim.GetHashCode();
            }
        }
    }
}