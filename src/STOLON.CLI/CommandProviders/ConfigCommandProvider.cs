using AsitLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;

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
            CLI.Debug.Log("opened user config file (user.ini).");
        }

        [Command("Print the path to the user.ini file.", isReadOnly: true)]
        public void Path()
        {
            Console.WriteLine(System.IO.Path.GetFullPath(_userIniPath));
        }

        [Command("Print the value of a key.", isReadOnly: true)]
        public void Get(string key)
        {
            string Represent(object o)
            {
                if (o is TomlArray tomlArray) return $"[{tomlArray.ToJoinedString(", ")}]";
                if (o is TomlTable tomlTable) return $"[{tomlTable.ToJoinedString(", ")}]";
                return o.ToString()!;
            }
            Console.WriteLine(Represent(CLI.Instance.Config.Get(key)));
        }

        [Command("Set the value of a key.")]
        public void Set(string key, string value)
        {
            CLI.Instance.Config.Set(key, value);
        }

        [Command("Reset a specific key.")]
        public void Reset(string key)
        {
            CLI.Instance.Config.Reset(key);
        }
    }
}
