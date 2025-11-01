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
        public RepositoryCommandProvider() : base("repo") { }

        private const string REPO_LINK = "https://github.com/JTnadrooi/STOLON";
        private const string REPO_LINK_GIT = "https://github.com/JTnadrooi/STOLON.git";

        [Command("Open the main repository page on Github.")]
        public void _M()
        {
            CLIHelpers.OpenLink(REPO_LINK);
        }

        [Command("Print repository .git link.", flags: CommandFlag.ReadOnly)]
        public void Link()
        {
            Console.WriteLine(REPO_LINK_GIT);
        }
    }
}
