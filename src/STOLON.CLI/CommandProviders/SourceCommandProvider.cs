using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STOLON.CLI.Command;

namespace STOLON.CLI
{
    public class SourceCommandProvider : CommandProvider
    {
        public SourceCommandProvider() : base("src") { }

        [Command("Open the local source code directory.", needsDev: true)]
        public void _M(string? subDir = null)
        {
            using Process p = Process.Start(Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "explorer.exe",
                PlatformID.Unix => "xdg-open",
                _ => "open"
            }, CLI.SourcePath! + subDir switch
            {
                "sl" => "STOLON\\",
                "cli" => "STOLON.CLI\\",
                null => string.Empty,
                _ => throw new ArgumentException("Accepted values; sl, cli", nameof(subDir))
            });
            CLI.Debug.Log($"Opened {(subDir == null ? string.Empty : $"'{subDir}'")} source directory.");
        }

        [Command("Prints the local source code directory path.", needsDev: true)]
        public void Path()
        {
            Console.WriteLine(CLI.SourcePath);
        }
    }
}
