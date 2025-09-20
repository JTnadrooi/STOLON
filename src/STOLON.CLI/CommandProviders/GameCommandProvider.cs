using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class GameCommandProvider : CommandProvider
    {
        public GameCommandProvider() : base("sl")
        {

        }
        [Command("Open the folder where STOLON is located.", inheritNamespace: false)]
        public void Start()
        {
            Process.Start("STOLON.exe");
            Console.WriteLine("Started STOLON.");
        }
    }
}
