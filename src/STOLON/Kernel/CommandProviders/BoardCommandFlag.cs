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
        public Entity TargetEntity { get; }

        public BoardCommandFlag(Board board, Entity targetEntity)
        {
            Board = board;
            TargetEntity = targetEntity;
        }
    }
}
