using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class DirectoryCommandProvider : CommandProvider
    {
        public DirectoryCommandProvider() : base("dir") { }

        [SLCommand("Open the directory where STOLON is located.", IsMain = true)]
        public void Main()
        {
            CLIHelpers.OpenDirectory(AppDomain.CurrentDomain.BaseDirectory);
            CLI.Logger.Log("Opened STOLON directory.");
        }

        [SLCommand("Print the path of the directory where STOLON is located.", Flags = CommandFlags.ReadOnly)]
        public void Path()
        {
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
