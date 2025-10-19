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

        public DevCommandProvider() : base("dev")
        {
        }

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
    }
}
