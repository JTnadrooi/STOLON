using System.Diagnostics.CodeAnalysis;

namespace STOLON
{
    public sealed class GameInfo
    {
        private readonly IInputManager _input;
        private readonly Camera2D _camera;

        public EntitySelection Selection { get; }

        public Vector2 ToWorldPosition(Vector2 position)
        {
            return _camera.Unproject(_input.Mouse.Position);
        }

        internal GameInfo(IInputManager input, Camera2D camera, EntitySelection selection)
        {
            _input = input;
            _camera = camera;

            Selection = selection;
        }
    }

    public interface IPlayer
    {
        bool TryGetMove(BoardState state, GameInfo gameInfo, [NotNullWhen(true)] out Move? move);
        bool HasWon(BoardState state, GameInfo gameInfo);
    }
}
