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
    public class VerboseOptionHandler : IOptionHandler
    {
        private bool _initialVerboseState;

        public VerboseOptionHandler()
        {
            _initialVerboseState = CLI.Instance.Debug.Silent;
        }

        public void PreCommand(ArgumentsInfo arguments)
        {
            if (arguments.Options.Contains("-v")) CLI.Instance.Debug.Silent = false;
        }

        public void PostCommand()
        {
            CLI.Instance.Debug.Silent = _initialVerboseState;
        }

        public OptionInfo[] GetOptionInfos()
            => [
                new OptionInfo("v", "verbose", "enabled logging"),
            ];
    }

}
