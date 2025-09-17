using AsitLib;
using STOLON.CLI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class CoreCommandProvider : CommandProvider
    {
        public CoreCommandProvider() : base("_STOLON_") { }

        [Command("Adds two values.", aliases: ["plus"])]
        public void Add(int a, int b = 0) => Console.WriteLine(a + b);
        [Command("Counts to a number.")]
        public void Count([Range(0, 10)] int target) => Console.WriteLine(Enumerable.Range(1, target).ToJoinedString(", "));
        //[Command]
        //public int Add(int a, int b, int c) => a + b + c;
        [Command("Greets someone with the specified name.")]
        public void Greet(string name, bool loud = false) => Console.WriteLine($"Hello, {(loud ? name.ToUpper() : name)}!");
        [Command("Prints the cli version.")]
        public void Version() => Console.WriteLine(CLI.VersionString);
        [Command("Displays help.", aliases: ["?", "h"])]
        public void Help(string? commandName = null)
        {
            void WriteCommand(CommandInfo cmd) => Console.WriteLine($"{cmd.Id}{(cmd.Ids.Count > 1 ? $"[{cmd.Ids.ToArray()[1..].ToJoinedString(", ")}]" : string.Empty)} {cmd.MethodInfo.GetParameters()
                    .Select(p => $"{p.ParameterType.Name.ToLower()}:{p.Name.ToLower()}{(p.HasDefaultValue ? ($"(default_value:{p.DefaultValue?.ToString() ?? "NULL"}) ") : " ")}").ToJoinedString("")}" +
                    $"# {cmd.Description}");

            if (commandName != null)
            {
                WriteCommand(CLI.Instance.Commands[commandName]);
                return;
            }

            Console.WriteLine("Available commands:");
            foreach (CommandInfo cmd in CLI.Instance.UniqueCommands.Values) WriteCommand(cmd);
        }
    }
}
