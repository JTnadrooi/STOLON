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
            _initialVerboseState = CLI.Instance.Debug.Silent;
        }

        public override void PreCommand(ArgumentsInfo arguments)
        {
            CLI.Instance.Debug.Silent = false;
        }

        public override void PostCommand()
        {
            CLI.Instance.Debug.Silent = _initialVerboseState;
        }
    }
}
