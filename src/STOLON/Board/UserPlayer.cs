using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public sealed class UserMove : Move
    {
        public UserMove(int tileX, int tileY)
        {

        }

        public override void Act(BoardState boardState)
        {
        }
    }

    public sealed class UserPlayer : IPlayer
    {
        private readonly IInputManager _input;

        public string Id { get; }

        public ImmutableArray<SearchTarget> SearchTargets { get; }

        public bool IsComputer { get; }

        public UserPlayer(IInputManager input, string id)
        {
            _input = input;

            Id = id;
            SearchTargets = SearchTarget.GetDefaultTargets();
        }

        public override bool Equals(object? obj) => obj is UserPlayer other && other.Id == Id;
        public override string? ToString() => Id;
        public override int GetHashCode() => Id.GetHashCode();

        public bool TryGetMove(BoardState boardState, GameInfo gameInfo, [NotNullWhen(true)] out Move? move)
        {
            Vector2 worldMousePos = gameInfo.ToWorldPosition(_input.Mouse.Position);

            if (_input.Mouse.IsClicked(MouseButton.Left) && _input.IsMouseOn<Board>())
            {
                //for (int x = 0; x < boardState.Tiles.GetLength(0); x++)
                //    for (int y = 0; y < boardState.Tiles.GetLength(1); y++)
                //        if (boardState.Tiles[x, y].HitBox.Contains(worldMousePos) && !boardState.Tiles[x, y].IsSolid())
                //        {
                //            move = new UserMove(x, y);
                //            return true;
                //        }
            }

            move = null;
            return false;
        }

        public bool HasWon(BoardState state, GameInfo gameInfo)
        {
            throw new NotImplementedException();
        }
    }
}
