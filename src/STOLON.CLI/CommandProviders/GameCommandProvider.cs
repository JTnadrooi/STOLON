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
        [Command("Start STOLON.", inheritNamespace: false)]
        public void Start()
        {
            using Process p = Process.Start("STOLON.exe");
            Console.WriteLine($"Started STOLON as '{p.ProcessName}'.");
        }
        [Command("Exit STOLON", inheritNamespace: false)]
        public void Exit()
        {
            Process[] processes = Process.GetProcessesByName("STOLON");
            if (processes.Length > 0)
                foreach (Process process in processes)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit();
                        Console.WriteLine($"STOLON process with PID {process.Id} has been terminated.");
                    }
                }
            else Console.WriteLine("No running STOLON processes found.");
        }
    }
}
