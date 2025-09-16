using AsitLib;
using STOLON.CLI;
using System;
using System.Collections.Generic;
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
        //[Command]
        //public int Add(int a, int b, int c) => a + b + c;
        [Command("Greets someone with the specified name.")]
        public void Greet(string name, bool loud = false) => Console.WriteLine($"Hello, {(loud ? name.ToUpper() : name)}!");
        [Command("Prints the cli version.")]
        public void Version() => Console.WriteLine(CLI.VersionString);
        [Command("Displays help.", aliases: ["?", "h"])]
        public void Help(string? commandName = null)
        {
            Console.WriteLine("Available commands:");
            foreach (CommandInfo cmd in CLI.Instance.UniqueCommands.Values)
            {
                Console.WriteLine($"{cmd.Id} {cmd.MethodInfo.GetParameters()
                    .Select(p => $"{p.ParameterType.Name.ToLower()}:{p.Name.ToLower()}{(p.HasDefaultValue ? ($"(default_value:{p.DefaultValue?.ToString() ?? "NULL"}) ") : " ")}").ToJoinedString("")}" +
                    $"# {cmd.Description}");
            }
        }
    }
}
