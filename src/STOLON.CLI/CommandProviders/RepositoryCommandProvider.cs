using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class RepositoryCommandProvider : CommandProvider
    {
        public RepositoryCommandProvider() : base("repo")
        {

        }

        private string _repoLink = "https://github.com/JTnadrooi/STOLON";
        private string _repoGitLink = "https://github.com/JTnadrooi/STOLON.git";

        [Command("Open the main repository page on Github.")]
        public void _M()
        {
            try
            {
                Process.Start(_repoLink);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo(_repoLink.Replace("&", "^&")) { UseShellExecute = true });
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", _repoLink);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", _repoLink);
                else throw;
            }
        }

        [Command("Print repository .git link.", isReadOnly: true)]
        public void Link()
        {
            Console.WriteLine(_repoGitLink);
        }
    }
}
