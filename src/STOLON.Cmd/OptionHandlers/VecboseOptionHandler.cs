using STOLON.Installer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.Cmd.CommandHelpers;

namespace STOLON.Cmd
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
