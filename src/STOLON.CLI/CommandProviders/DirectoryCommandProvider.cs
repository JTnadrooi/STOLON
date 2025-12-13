using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class DirectoryCommandProvider : CommandGroup
    {
        public DirectoryCommandProvider() : base("dir", CLI.InfoFactory, nameOfMainMethod: nameof(Main)) { }

        [FlaggedCommand("Opens the directory where STOLON is located.")]
        public void Main()
        {
            CLIHelpers.OpenDirectory(AppDomain.CurrentDomain.BaseDirectory);
            CLI.Logger.Log("Opened STOLON directory.");
        }

        [FlaggedCommand("Prints the path of the directory where STOLON is located.", Flags = CommandFlags.ReadOnly)]
        public void Path()
        {
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
