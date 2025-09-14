using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
    public interface IOptionHandler
    {
        public void PreCommand(ArgumentsInfo arguments);
        public void PostCommand();
    }
}
