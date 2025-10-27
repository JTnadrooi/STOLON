using STOLON.CLI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STOLON.CLI.Command;
using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
    public class QuitFlagHandler : FlagHandler
    {
        public QuitFlagHandler() : base("quit", "Quit after command.", "q") { }

        public override void PreCommand(ArgumentsInfo arguments) { }
        public override void PostCommand()
        {
            CLI.Instance.Exit(0);
        }
    }
}
