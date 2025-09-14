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
            _initialVerboseState = CommandHandler.Instance.Debug.Silent;
        }

        public void PreCommand(ArgumentsInfo arguments)
        {
            if (arguments.Options.Contains("-v")) CommandHandler.Instance.Debug.Silent = false;
        }

        public void PostCommand()
        {
            CommandHandler.Instance.Debug.Silent = _initialVerboseState;
        }
    }

}
