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

        private string _userIniPath = "user.ini";

        [Command("Open user.ini file.")]
        public void _M()
        {
            Process.Start("notepad.exe", _userIniPath);
            Console.WriteLine("Opened user config file (user.ini).");
        }

        [Command("Print the path to the user.ini file.", isReadOnly: true)]
        public void Path()
        {
            Console.WriteLine(System.IO.Path.GetFullPath(_userIniPath));
        }
    }
}
