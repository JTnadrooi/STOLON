using AsitLib.CommandLine;
using System.Diagnostics;


namespace STOLON.CLI
{
    public class GameCommandProvider : CommandGroup
    {
        private readonly IRichLogger _logger;

        public GameCommandProvider() : base("sl")
        {
            _logger = STOLON.Services.Resolve<IRichLogger>();
        }

        [FlaggedCommand("Starts STOLON.")]
        public void Start()
        {
            using Process p = Process.Start("STOLON.exe");
            _logger.Log($"started STOLON as '{p.ProcessName}'.");
        }

        [FlaggedCommand("Exits STOLON.")]
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
                        _logger.Log($"STOLON process with PID {process.Id} has been terminated.");
                    }
                }
            else Console.WriteLine("No running STOLON processes found.");
        }

        [FlaggedCommand("Sets the STOLON version.", Id = "version-set", Flags = CommandFlags.DevOnly)]
        public void SetVersion(string newVer) => File.WriteAllText(Path.Combine(CLI.SourceResourcesPath!, ".version"), newVer);
    }
}
