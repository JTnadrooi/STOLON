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
    public interface IOptionHandler
    {
        public void HandleOption(ArgumentsInfo arguments);
        public void RestoreInitialState();
    }

    public class SilentOptionHandler : IOptionHandler
    {
        private bool _initialSilentState;

        public SilentOptionHandler(bool initialSilentState)
        {
            _initialSilentState = initialSilentState;
        }

        public void HandleOption(ArgumentsInfo arguments)
        {
            if (arguments.Options.Contains("-s")) CommandHandler.Instance.Debug.Silent = true;
        }

        public void RestoreInitialState()
        {
            CommandHandler.Instance.Debug.Silent = _initialSilentState;
        }
    }

}
