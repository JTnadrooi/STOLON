using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class SourceCommandProvider : CommandProvider
    {
        public SourceCommandProvider() : base("src") { }

        [Command("Open the local source code directory.", needsDev: true)]
        public void _M()
        {
            using Process p = Process.Start(Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "explorer.exe",
                PlatformID.Unix => "xdg-open",
                _ => "open"
            }, CLI.SourcePath!);
            CLI.Debug.Log("Opened source directory.");
        }

        [Command("Prints the local source code directory path.", needsDev: true)]
        public void Path()
        {
            Console.WriteLine(CLI.SourcePath);
        }
    }
}
