using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    public static class BuildHelper
    {
        /// <summary>
        /// Copy the <paramref name="src"/> to the <paramref name="dest"/> and make needed directories.
        /// </summary>
        public static void Copy(string src, string dest)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(src, dest, true);
        }
    }
}
