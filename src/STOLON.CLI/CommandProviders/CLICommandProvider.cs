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
        [Command("Print the cli version.")]
        public void Version() => Console.WriteLine(CLI.VersionString);
        [Command("Display help.", aliases: ["?", "h"], useProviderNamespace: false)]
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
        [Command("Exit the program.")]
        public void Exit() => Environment.Exit(0);
        [Command("Bump the .cli-version to .version.")]
        public void Bump()
        {
            string slVer = File.ReadAllText(".version");
            File.WriteAllText(".cli-version", slVer);
            Console.WriteLine($"Bumped .cli-version to {slVer}.");
        }
    }
}
