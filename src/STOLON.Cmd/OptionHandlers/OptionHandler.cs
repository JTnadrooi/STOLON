using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.Cmd.CommandHelpers;

namespace STOLON.Cmd
{
    public interface IOptionHandler
    {
        public void PreCommand(ArgumentsInfo arguments);
        public void PostCommand();
    }
}
