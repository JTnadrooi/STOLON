using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class DirectoryCommandProvider : CommandProvider
    {
        public DirectoryCommandProvider() : base("dir") { }

        [Command("Open the directory where STOLON is located.")]
        public void _M()
        {
            using Process p = Process.Start(Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "explorer.exe",
                PlatformID.Unix => "xdg-open",
                _ => "open"
            }, AppDomain.CurrentDomain.BaseDirectory);
            CLI.Debug.Log("Opened STOLON directory.");
        }

        [Command("Print the path of the directory where STOLON is located.", isReadOnly: true)]
        public void Path()
        {
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
