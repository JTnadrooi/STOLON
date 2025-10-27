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
    public class SilentFlagHandler : FlagHandler
    {
        private TextWriter? originalOut;
        private TextWriter? originalErr;

        public SilentFlagHandler() : base("silent", "Silence stdout.", "s")
        {
            originalOut = null;
            originalErr = null;
        }

        public override void PreCommand(ArgumentsInfo arguments)
        {
            originalOut = Console.Out;
            originalErr = Console.Error;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }

        public override void PostCommand()
        {
            if (originalOut != null)
                Console.SetOut(originalOut);
            if (originalErr != null)
                Console.SetError(originalErr);
        }
    }
}
