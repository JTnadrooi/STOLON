using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class BoardCommandProvider : CommandProvider
    {
        private readonly CommandManager _commandManager;

        public BoardCommandProvider(CommandManager commandManager) : base("board")
        {
            _commandManager = commandManager;
        }

        private Point GetPointFromCoords(string coords) // ex: a1
        {
            if (string.IsNullOrWhiteSpace(coords) || coords.Length != 2)
                throw new CommandArgumentException("Invalid coordinates");

            char file = char.ToLower(coords[0]);
            char rank = coords[1];

            if (file < 'a' || file > 'h' || rank < '1' || rank > '8')
                throw new CommandArgumentException("Invalid coordinates");

            int x = file - 'a';
            int y = rank - '1';

            return new Point(x, y);
        }

        [KernelCommand("Places a token at the specified position.", Aliases = ["pat"], RequiredFlag = typeof(BoardCommandFlag))]
        public void PlaceAt(string coords)
        {
            BoardCommandFlag flag = (BoardCommandFlag)_commandManager.ActiveFlag!;

if (flag.IsCurrentEntityUserControlled)
            {
            Point movePos = Board.GetPointFromCoords(coords);
            IMove move = new GravityAffectedMove(movePos.X, movePos.Y);

            move.Apply(flag.Board.State, flag.Board.State.CurrentEntity);
}
        }
    }
}
