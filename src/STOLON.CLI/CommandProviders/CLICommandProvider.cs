using AsitLib;
using STOLON.CLI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class CLICommandProvider : CommandProvider
    {
        public CLICommandProvider() : base("cli") { }

        [Command("Adds two values.", aliases: ["plus"], useProviderNamespace: false)]
        public void Add(int a, int b = 0) => Console.WriteLine(a + b);
        [Command("Counts to a number.", useProviderNamespace: false)]
        public void Count([Range(0, 10)] int target) => Console.WriteLine(Enumerable.Range(1, target).ToJoinedString(", "));
        //[Command]
        //public int Add(int a, int b, int c) => a + b + c;
        [Command("Greets someone with the specified name.", useProviderNamespace: false)]
        public void Greet(string? name = null, bool loud = false, bool ahoy = false)
        {
            string greeting = string.IsNullOrEmpty(name) ? "Greeting" : (ahoy ? "Ahoy" : "Hello");
            string finalName = name ?? string.Empty;

            if (loud)
            {
                greeting = greeting.ToUpper();
                finalName = finalName.ToUpper();
            }

            string namePart = string.IsNullOrEmpty(finalName) ? string.Empty : $" {finalName}";
            Console.WriteLine($"{greeting}{namePart}!");
        }


        [Command("Prints the cli version.")]
        public void Version() => Console.WriteLine(CLI.VersionString);
        [Command("Displays help.", aliases: ["?", "h"], useProviderNamespace: false)]
        public void Help(string? filter = null)
        {
            void WriteCommand(CommandInfo cmd) => Console.WriteLine($"{cmd.Id}{(cmd.HasAliases ? $"[{cmd.Ids.Skip(1).ToJoinedString(", ")}]" : string.Empty)} {cmd.MethodInfo.GetParameters()
                    .Select(p => $"{p.ParameterType.Name.ToLower()}:{p.Name.ToLower()}{(p.HasDefaultValue ? ($"(default_value:{p.DefaultValue?.ToString() ?? "NULL"}) ") : " ")}").ToJoinedString("")}" +
                    $"# {cmd.Description}");
            IEnumerable<CommandInfo> toPrint;

            if (filter == null) toPrint = CLI.Instance.UniqueCommands.Values.OrderBy(c => c.Id);
            else if (filter.EndsWith("\\"))
            {
                string providerId = filter[..^1];
                if (!CLI.Instance.Providers.ContainsKey(providerId)) throw new InvalidOperationException($"No CommandProvider with id '{providerId}' found.");
                toPrint = CLI.Instance.UniqueCommands.Values.Where(c => c.Source.Id == providerId);
            }
            else toPrint = [CLI.Instance.Commands[filter]];

            foreach (CommandInfo cmd in toPrint) WriteCommand(cmd);
        }
        [Command("Opens the folder where the executable is located.", idOverride: "dir", useProviderNamespace: false)]
        public void Directory()
        {
            Process.Start(Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "explorer.exe",
                PlatformID.Unix => "xdg-open",
                _ => "open"
            }, AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("Opened STOLON main directory.");
        }
    }
}
