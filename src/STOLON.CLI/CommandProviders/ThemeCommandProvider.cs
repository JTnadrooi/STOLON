using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class ThemeCommandProvider : CommandGroup
    {
        public ThemeCommandProvider() : base("theme", CLI.InfoFactory, nameOfMainMethod: nameof(Main))
        {

        }

        [FlaggedCommand("Prints the name of the color theme.", Flags = CommandFlags.ReadOnly)]
        public void Main()
        {
            Console.WriteLine(CLI.Instance.Config.Get<string>("graphics.theme"));
        }

        [FlaggedCommand("Sets the color theme.")]
        public void Set(string themeId)
        {
            CLI.Instance.Config.Set("graphics.theme", themeId);
        }

        [FlaggedCommand("Sets the color theme.")]
        public void Reset(string themeId)
        {
            CLI.Instance.Config.Reset("graphics.theme");
        }

        [FlaggedCommand("Prints the path of the color theme.")]
        public void Path(string? themeId = null)
        {
            throw new NotImplementedException();
        }
    }
}
