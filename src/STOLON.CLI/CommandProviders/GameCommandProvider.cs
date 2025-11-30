using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class GameCommandProvider : CommandProvider
    {
        public GameCommandProvider() : base("sl") { }

        [SLCommand("Start STOLON.")]
        public void Start()
        {
            using Process p = Process.Start("STOLON.exe");
            CLI.Logger.Log($"started STOLON as '{p.ProcessName}'.");
        }

        [SLCommand("Exit STOLON.")]
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
                        CLI.Logger.Log($"STOLON process with PID {process.Id} has been terminated.");
                    }
                }
            else Console.WriteLine("No running STOLON processes found.");
        }

        [SLCommand("Set the STOLON version.", Id = "version-set", Flags = CommandFlags.DevOnly)]
        public void SetVersion(string newVer) => File.WriteAllText(Path.Combine(CLI.SourceResourcesPath!, ".version"), newVer);
    }
}
