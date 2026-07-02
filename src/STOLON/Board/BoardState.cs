using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public sealed class BoardState
    {
        public Point Dimensions => _dimensions;
        public Tile[,] Tiles => _tiles;
        public Entity CurrentEntity => _currentPlayerIndex == 0 ? Player1 : Player2;
        public Entity Player1 => _player1;
        public Entity Player2 => _player2;

        public readonly Stack<(IMove, Entity)> _moveStack;
        private readonly Tile[,] _tiles;
        private readonly Entity _player1;
        private readonly Entity _player2;
        private readonly Point _dimensions;

        public int _currentPlayerIndex;

        public BoardState(Tile[,] tiles, Entity player1, Entity player2, int currentPlayer = 0)
        {
            _tiles = tiles;
            _player1 = player1;
            _player2 = player2;
            _dimensions = new Point(tiles.GetLength(0), tiles.GetLength(1));
            _moveStack = new Stack<(IMove, Entity)>();

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

            return new BoardState(resultTiles, _player1.NodeCopy(), _player2.NodeCopy(), _currentPlayerIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Tile GetTileAt(Point p) => ref _tiles[p.X, p.Y];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public bool SearchFor(ReadOnlySpan<SearchTarget> searchTargets, int playerId = -1)
        {
            foreach (SearchTarget target in searchTargets)
                if (SearchFor(target, playerId)) return true;
            return false;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)] // maybe remove if method becomes too big. (method became too big)
        public bool SearchFor(in SearchTarget target, int playerId = -1)
        {
            for (int x = 0; x < _dimensions.X; x++)
                for (int y = 0; y < _dimensions.Y; y++)
                {
                    int occupiedPlayerId = _tiles[x, y].GetOccupiedByPlayerIndex();

                    if (occupiedPlayerId == -1) continue;
                    if (playerId != -1 && playerId != occupiedPlayerId) continue;

                    int score = 0;

                    for (int i = 0; i < target.Nodes.Length; i++)
                    {
                        if (TryGetTileAt(new Point(target.Nodes[i].X + x, target.Nodes[i].Y + y), out Tile? tile))
                        {
                            if (tile.Value.GetOccupiedByPlayerIndex() == occupiedPlayerId) score++;
                        }
                    }

                    if (score == target.Nodes.Length) return true;
                }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntityIndex(Entity entity) => entity == Player1 ? 0 : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlterRemove(Point tilePos, TileAttributes attributesToRemove)
        {
            Tiles[tilePos.X, tilePos.Y] = new Tile(tilePos, Tiles[tilePos.X, tilePos.Y].Attributes & ~attributesToRemove);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlterAdd(Point tilePos, TileAttributes additionalAttributes)
        {
            Tiles[tilePos.X, tilePos.Y] = new Tile(tilePos, Tiles[tilePos.X, tilePos.Y].Attributes | additionalAttributes);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Alter(Point tilePos, TileAttributes newAttributes)
        {
            Tiles[tilePos.X, tilePos.Y] = new Tile(tilePos, newAttributes);
            return true;
        }

        public BoardPreview GetPreview(Font2D font)
            => new BoardPreview(this, font);

        public void RegisterMove(IMove move, Entity performer)
        {
            _moveStack.Push((move, performer));
            Update();
        }

        // not every frame ofc
        private void Update()
        {
            for (int x = 0; x < Tiles.GetLength(0); x++)
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    if (Tiles[x, y].HasAttribute(TileAttributes.Disabled0))
                    {
                        AlterRemove(new Point(x, y), TileAttributes.Disabled);
                    }
                    if (Tiles[x, y].HasAttribute(TileAttributes.Disabled1))
                    {
                        AlterRemove(new Point(x, y), TileAttributes.Disabled);
                        AlterAdd(new Point(x, y), TileAttributes.Disabled0);
                    }
                }
        }

        public void Undo()
        {
            (IMove move, Entity entity) = _moveStack.Pop();
            move.Undo(this, entity);
        }

        public static BoardState GetDefault(Entity player1, Entity player2)
            => new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), player1, player2);
    }
}