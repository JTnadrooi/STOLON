using AsitLib;
using AsitLib.CommandLine;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;

namespace STOLON.CLI
{
    public class ConfigCommandProvider : CommandGroup
    {
        public ConfigCommandProvider() : base("conf", CLI.InfoFactory, nameOfMainMethod: nameof(Main)) { }

        private string _userIniPath = "user.ini";

        [FlaggedCommand("Opens the user.ini file.")]
        public void Main()
        {
            Process.Start("notepad.exe", _userIniPath);
            STOLON.Logger.Log("opened user config file (user.ini).");
        }

        [FlaggedCommand("Prints the path to the user.ini file.", Flags = CommandFlags.ReadOnly)]
        public void Path()
        {
            Console.WriteLine(System.IO.Path.GetFullPath(_userIniPath));
        }

        [FlaggedCommand("Prints the value of a key.", Flags = CommandFlags.ReadOnly)]
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

        [FlaggedCommand("Sets the value of a key.")]
        public void Set(string key, string value)
        {
            CLI.Instance.Config.Set(key, value);
        }

        [FlaggedCommand("Resets a specific key.")]
        public void Reset(string key)
        {
            CLI.Instance.Config.Reset(key);
        }

        [FlaggedCommand("Resets a specific key.", Flags = CommandFlags.ReadOnly)]
        public void Keys()
        {
            string Format(string key, object value, object defaultValue) => $"Key = {key}, Value = {value ?? "null"}, DefaultValue = {defaultValue ?? "null"}";
            foreach (KeyValuePair<string, object> kvp in CLI.Instance.Config.TomlValues)
            {
                Console.WriteLine(kvp.Value switch
                {
                    TomlArray a => Format(kvp.Key, $"[{a.ToJoinedString(", ")}]", $"[{((Array)CLI.Instance.Config.Defaults[kvp.Key]).Cast<object>().ToJoinedString(", ")}]"),
                    _ => Format(kvp.Key, kvp.Value, CLI.Instance.Config.Defaults[kvp.Key]),
                });
            }
        }
    }
}
