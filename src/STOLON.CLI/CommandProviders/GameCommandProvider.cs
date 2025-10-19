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
        public GameCommandProvider() : base("sl") { }

        [Command("Start STOLON.")]
        public void Start()
        {
            using Process p = Process.Start("STOLON.exe");
            CLI.Debug.Log($"started STOLON as '{p.ProcessName}'.");
        }

        [Command("Exit STOLON.")]
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
                        CLI.Debug.Log($"STOLON process with PID {process.Id} has been terminated.");
                    }
                }
            else Console.WriteLine("No running STOLON processes found.");
        }

        [Command("Print the STOLON version.")]
        public void Version() => Console.WriteLine(File.ReadAllText(".version"));

        [Command("Set the STOLON version.", "version-set", needsDev: true)]
        public void SetVersion(string newVer) => File.WriteAllText(Path.Combine(CLI.SourcePostBuildPath!, ".version"), newVer);
    }
}
