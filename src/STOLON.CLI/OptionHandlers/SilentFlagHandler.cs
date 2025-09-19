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
    public class SilentFlagHandler : FlagHandler
    {
        private TextWriter? originalOut;

        public SilentFlagHandler() : base("silent", "Silences stdout.", "s")
        {
            originalOut = null;
        }

        public override void PreCommand(ArgumentsInfo arguments)
        {
            originalOut = Console.Out;
            Console.SetOut(TextWriter.Null);
        }

        public override void PostCommand() => Console.SetOut(originalOut!);
    }
}
