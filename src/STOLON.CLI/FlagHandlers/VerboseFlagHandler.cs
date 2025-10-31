using STOLON.CLI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
    public class VerboseFlagHandler : FlagHandler
    {
        private bool _initialVerboseState;

        public VerboseFlagHandler() : base("verbose", "Enable verbose logging.", "v")
        {
            _initialVerboseState = CLI.Debug.Silent;
        }

        public override void PreCommand(ArgumentsInfo arguments)
        {
            CLI.Debug.Silent = false;
        }

        public override void PostCommand()
        {
            CLI.Debug.Silent = _initialVerboseState;
        }
    }
}
