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

        [KernelCommand("Places a token at the specified position.", Aliases = ["@"], RequiredFlag = typeof(BoardCommandFlag))]
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
