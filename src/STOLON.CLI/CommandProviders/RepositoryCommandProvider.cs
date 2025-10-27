using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using STOLON.CLI.Command;

namespace STOLON.CLI
{
    public class RepositoryCommandProvider : CommandProvider
    {
        public RepositoryCommandProvider() : base("repo") { }

        private const string REPO_LINK = "https://github.com/JTnadrooi/STOLON";
        private const string REPO_LINK_GIT = "https://github.com/JTnadrooi/STOLON.git";

        [Command("Open the main repository page on Github.")]
        public void _M()
        {
            try
            {
                Process.Start(REPO_LINK);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo(REPO_LINK.Replace("&", "^&")) { UseShellExecute = true });
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", REPO_LINK);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", REPO_LINK);
                else throw;
            }
        }

        [Command("Print repository .git link.",  flags: CommandFlag.ReadOnly)]
        public void Link()
        {
            Console.WriteLine(REPO_LINK_GIT);
        }
    }
}
