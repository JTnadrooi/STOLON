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
        private readonly Shell _shell;

        public BoardCommandProvider(CommandManager commandManager, Shell shell) : base("board")
        {
            _commandManager = commandManager;
            _shell = shell;
        }

        private void ThrowIfNotUserControlled(BoardCommandFlag flag)
        {
            if (!flag.IsCurrentEntityUserControlled)
            {
                throw new InvalidOperationException("Current entity is not user controlled.");
            }
        }

        [KernelCommand("Places a token at the specified coordinates.", Aliases = ["@"], RequiredFlag = typeof(BoardCommandFlag))]
        public void PlaceAt(string coords)
        {
            coords = coords.ToLower();

            BoardCommandFlag flag = (BoardCommandFlag)_commandManager.ActiveFlag!;

            ThrowIfNotUserControlled(flag);

            Point movePos = Board.GetPointFromCoords(coords);
            GravityAffectedMove move = new GravityAffectedMove(movePos.X, movePos.Y);

            move.Apply(flag.Board.State, flag.Board.State.CurrentEntity);

            _shell.WriteLine($"Placed token at '{coords}'.");
        }

        [KernelCommand("Initiates the current entity's ability.", Aliases = ["a@"], RequiredFlag = typeof(BoardCommandFlag))]
        public void AbiltyAt(string arg)
        {
            arg = arg.ToLower();

            BoardCommandFlag flag = (BoardCommandFlag)_commandManager.ActiveFlag!;

            ThrowIfNotUserControlled(flag);

            switch (flag.Board.State.CurrentEntity)
            {
                case SiloEntity silo:
                    char column = arg.Length > 1 ? throw new ArgumentException("Invalid column coords length.", nameof(arg)) : arg[0];

                    ColumnDisableMove move = new ColumnDisableMove(Board.GetColumnIndexFromChar(column));
                    move.Apply(flag.Board.State, flag.Board.State.CurrentEntity);
                    _shell.WriteLine($"Disabled column '{column}' for 1 move.");
                    break;
                default:
                    throw new InvalidOperationException("Ability not supported.");
            }

        }
    }
}
