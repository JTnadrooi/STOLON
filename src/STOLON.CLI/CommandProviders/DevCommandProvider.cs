using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STOLON.CLI.Command;

namespace STOLON.CLI.CommandProviders
{
    public class DevCommandProvider : CommandProvider
    {
        public DevCommandProvider() : base("dev") { }

        [Command($"Prints a value indicating if the {CLI.BUILD_INFO_DIRECTORY} directory is found and valid.")]
        public void _M()
        {
            Console.WriteLine(CLI.IsDev);
        }
    }
}
