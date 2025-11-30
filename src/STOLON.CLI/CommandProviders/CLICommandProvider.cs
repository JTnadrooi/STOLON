using AsitLib;
using AsitLib.CommandLine;
using STOLON.CLI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class CLICommandProvider : CommandProvider
    {
        public CLICommandProvider() : base("cli") { }

        [SLCommand("Display info about the STOLON.CLI.", Flags = CommandFlags.ReadOnly, IsMain = true)]
        public void Main()
        {
            Console.WriteLine($"Command_Count={CLI.Engine.UniqueCommands.Count}");
            Console.WriteLine($"Provider_Count={CLI.Engine.Providers.Count}");
            Console.WriteLine($"Is_Dev={CLI.IsDev}");
            Console.WriteLine($"STOLON_Version={STOLON.Version}");
        }

        [SLCommand("Exit the program.")]
        public void Exit() => CLI.Instance.Exit();
    }
}
