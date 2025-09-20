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
        public DirectoryCommandProvider() : base("dir")
        {

        }
        [Command("Print the folder where STOLON is located.")]
        public void _M()
        {
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        }
        [Command("Open the folder where STOLON is located.")]
        public void Open()
        {
            Process.Start(Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "explorer.exe",
                PlatformID.Unix => "xdg-open",
                _ => "open"
            }, AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("Opened STOLON directory.");
        }
    }
}
