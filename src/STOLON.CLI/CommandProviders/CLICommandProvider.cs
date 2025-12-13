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

        [FlaggedCommand("Displays info about the STOLON.CLI.", Id = "cli", Aliases = ["info"], Flags = CommandFlags.ReadOnly)]
        public void Info()
        {
            Console.WriteLine($"Command_Count={CLI.Engine.UniqueCommands.Count}");
            Console.WriteLine($"Provider_Count={CLI.Engine.Providers.Count}");
            Console.WriteLine($"Is_Dev={CLI.IsDev}");
            Console.WriteLine($"STOLON_Version={STOLON.Version}");
        }

        [FlaggedCommand("Exits the program.")]
        public void Exit() => CLI.Instance.Exit();

        [FlaggedCommand("Prints the STOLON/CLI version.", Aliases = ["v"], Flags = CommandFlags.ReadOnly, IsGenericFlag = true)]
        public void Version() => Console.WriteLine(STOLON.Version);
    }
}
