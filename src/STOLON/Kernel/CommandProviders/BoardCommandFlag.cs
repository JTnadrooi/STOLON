using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class BoardCommandFlag : CommandFlag
    {
        public Board Board { get; }

        public BoardCommandFlag(Board board, Entity targetEntity)
        {
            Board = board;
        }

        public bool IsCurrentEntityUserControlled => Board.State.CurrentEntity.MoveProvider is UserMoveProvider;
    }
}
