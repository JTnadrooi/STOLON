using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class ThemeCommandProvider : CommandProvider
    {
        public ThemeCommandProvider() : base("theme")
        {

        }

        [SLCommand("Print the name of the color theme.", Flags = CommandFlags.ReadOnly, IsMain = true)]
        public void Main()
        {
            Console.WriteLine(CLI.Instance.Config.Get<string>("graphics.theme"));
        }

        [SLCommand("Set the color theme.")]
        public void Set(string themeId)
        {
            CLI.Instance.Config.Set("graphics.theme", themeId);
        }

        [SLCommand("Set the color theme.")]
        public void Reset(string themeId)
        {
            CLI.Instance.Config.Reset("graphics.theme");
        }

        [SLCommand("Print the path of the color theme.")]
        public void Path(string? themeId = null)
        {
            throw new NotImplementedException();
        }
    }
}
