using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class ConfigCommandProvider : CommandProvider
    {
        public ConfigCommandProvider() : base("conf") { }

        [Command("Open user.ini file.")]
        public void Open()
        {
            Process.Start("notepad.exe", "user.ini");
            Console.WriteLine("Opened user config file (user.ini).");
        }
    }
}
