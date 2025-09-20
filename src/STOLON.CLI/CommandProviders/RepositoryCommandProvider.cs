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

        [Command("Print repository link.")]
        public void _M()
        {
            Console.WriteLine("https://github.com/JTnadrooi/STOLON.git");
        }

        [Command("Open the main repository page on Github.")]
        public void Open()
        {
            string url = "https://github.com/JTnadrooi/STOLON";
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", url);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", url);
                else throw;
            }
        }
    }
}
