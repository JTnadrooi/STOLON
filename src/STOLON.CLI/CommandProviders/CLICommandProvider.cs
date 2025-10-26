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

        [Command("Display info about the STOLON.CLI.", isReadOnly: true)]
        public void _M()
        {
            Console.WriteLine($"Command_Count={CLI.Instance.UniqueCommands.Count}");
            Console.WriteLine($"Provider_Count={CLI.Instance.Providers.Count}");
            Console.WriteLine($"Is_Dev={CLI.IsDev}");
            Console.WriteLine($"CLI_Version={CLI.Version}");
            Console.WriteLine($"STOLON_Version={STOLON.Version}");
            //if (CLI.IsDev)
            //{
            //    Console.WriteLine($"SourcePath={CLI.SourcePath}");
            //}
        }

        [Command("Print the cli version.", isReadOnly: true)]
        public void Version() => Console.WriteLine(CLI.Version);

        [Command("Display help.", aliases: ["?", "h"], inheritNamespace: false, isReadOnly: true)]
        public void Help(string? filter = null)
        {
            void WriteCommand(CommandInfo cmd) => Console.WriteLine($"{cmd.Id}{(cmd.HasAliases ? $"[{cmd.Ids.Skip(1).ToJoinedString(", ")}]" : string.Empty)} {cmd.MethodInfo.GetParameters()
                    .Select(p => $"{p.ParameterType.Name.ToLower()}:{p.Name.ToLower()}{(p.HasDefaultValue ? ($"(default_value:{p.DefaultValue?.ToString() ?? "NULL"}) ") : " ")}").ToJoinedString("")}" +
                    $"# {cmd.Description}");
            void WriteProvider(CommandProvider provider) => Console.WriteLine($"{provider.FullNamespace}{(provider.IsNested ? $"[{provider.Namespace}]" : string.Empty)} # {CLI.Instance.Commands.Values.Where(cmd => cmd.Provider == provider).Count()} commands.");

            IEnumerable<CommandInfo> toPrint;

            if (filter == null) toPrint = CLI.Instance.UniqueCommands.Values.OrderBy(c => c.Id);
            else if (filter == "\\")
            {
                foreach (CommandProvider provider in CLI.Instance.Providers.Values) WriteProvider(provider);
                return;
            }
            else if (filter.EndsWith("\\"))
            {
                string providerId = filter[..^1];
                if (!CLI.Instance.Providers.ContainsKey(providerId)) throw new InvalidOperationException($"No CommandProvider with id '{providerId}' found.");
                toPrint = CLI.Instance.UniqueCommands.Values.Where(c => c.Provider.Namespace == providerId);
            }
            else toPrint = [CLI.Instance.Commands[filter]];

            foreach (CommandInfo cmd in toPrint) WriteCommand(cmd);
        }

        [Command("Exit the program.")]
        public void Exit() => Environment.Exit(0);

        [Command("Bump the .cli-version to .version.", needsDev: true)]
        public void Bump()
        {
            string slVer = File.ReadAllText(Path.Combine(CLI.SourceResourcesPath!, ".version"));
            File.WriteAllText(Path.Combine(CLI.SourceResourcesPath!, ".cli-version"), slVer);
            CLI.Debug.Log($"bumped .cli-version to {slVer}.");
        }
    }
}
