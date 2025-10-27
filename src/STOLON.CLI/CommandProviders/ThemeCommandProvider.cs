using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STOLON.CLI.Command;

namespace STOLON.CLI
{
    public class ThemeCommandProvider : CommandProvider
    {
        public ThemeCommandProvider() : base("theme")
        {

        }

        [Command("Print the name of the color theme.",  flags: CommandFlag.ReadOnly)]
        public void _M()
        {
            Console.WriteLine(CLI.Instance.Config.Get<string>("graphics.theme"));
        }

        [Command("Set the color theme.")]
        public void Set(string themeId)
        {
            CLI.Instance.Config.Set("graphics.theme", themeId);
        }

        [Command("Set the color theme.")]
        public void Reset(string themeId)
        {
            CLI.Instance.Config.Reset("graphics.theme");
        }

        [Command("Print the path of the color theme.")]
        public void Path(string themeId)
        {
            throw new NotImplementedException();
        }
    }
}
