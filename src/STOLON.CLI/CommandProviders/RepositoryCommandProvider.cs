using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class RepositoryCommandProvider : CommandProvider
    {
        public RepositoryCommandProvider() : base("repo") { }

        private const string REPO_LINK = "https://github.com/JTnadrooi/STOLON";
        private const string REPO_LINK_GIT = "https://github.com/JTnadrooi/STOLON.git";

        [SLCommand("Open the main repository page on Github.", IsMain = true)]
        public void Main()
        {
            CLIHelpers.OpenLink(REPO_LINK);
        }

        [SLCommand("Print repository .git link.", Flags = CommandFlags.ReadOnly)]
        public void Link()
        {
            Console.WriteLine(REPO_LINK_GIT);
        }
    }
}
