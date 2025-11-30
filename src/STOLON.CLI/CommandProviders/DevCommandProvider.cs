using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;


namespace STOLON.CLI.CommandProviders
{
    public class DevCommandProvider : CommandProvider
    {
        public DevCommandProvider() : base("dev") { }

        [SLCommand($"Prints a value indicating if the {CLI.BUILD_INFO_DIRECTORY} directory is found and valid.", Flags = CommandFlags.ReadOnly, IsMain = true)]
        public void Main()
        {
            Console.WriteLine(CLI.IsDev);
        }
    }
}
