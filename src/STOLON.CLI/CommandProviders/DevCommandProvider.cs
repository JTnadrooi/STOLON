using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.CommandProviders
{
    public class DevCommandProvider : CommandProvider
    {
        public const string BUILD_INFO_DIRECTORY = "_BUILDINFO";

        public DevCommandProvider() : base("dev") { }

        private void ThrowIfNotDev()
        {
            if (!Directory.Exists(BUILD_INFO_DIRECTORY))
            {
                throw new InvalidOperationException();
            }
            CLI.Debug.Log("devcheck succes.");
        }

        [Command($"Prints a value indicating if the {BUILD_INFO_DIRECTORY} directory is found and valid.")]
        public void _M()
        {
            try
            {
                ThrowIfNotDev();
                Console.WriteLine(true);
            }
            catch (InvalidOperationException e)
            {
                CLI.Debug.Log("User is not a dev: " + e.Message + ".");
                Console.WriteLine(false);
            }
        }

        public class SourceCommandProvider : CommandProvider
        {
            private const string SOURCE_PATH = "./../../src";

            public SourceCommandProvider() : base("src") { }

            [Command("Open the local source code directory.")]
            public void _M()
            {
                using Process p = Process.Start(Environment.OSVersion.Platform switch
                {
                    PlatformID.Win32NT => "explorer.exe",
                    PlatformID.Unix => "xdg-open",
                    _ => "open"
                }, System.IO.Path.GetFullPath(SOURCE_PATH));
                CLI.Debug.Log("Opened source directory.");
            }

            [Command("Prints the local source code directory path.")]
            public void Path()
            {
                Console.WriteLine(System.IO.Path.GetFullPath(SOURCE_PATH));
            }
        }
    }
}
