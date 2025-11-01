using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public static class CLIHelpers
    {
        public static void OpenDirectory(string path)
        {
            using Process p = Process.Start(Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "explorer.exe",
                PlatformID.Unix => "xdg-open",
                _ => "open"
            }, path);
        }

        public static void OpenFile(string path)
        {
            using Process p = Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"")) ?? throw new InvalidOperationException("File could not be opened, process could not be started");
        }

        public static void OpenLink(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo(link.Replace("&", "^&")) { UseShellExecute = true });
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", link);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", link);
                else throw;
            }
        }
    }
}
