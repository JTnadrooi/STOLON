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
        /// Gets a value indicating if the target item (<paramref name="to"/>) needs to be rebuild.
        /// </summary>
        /// <param name="from">The source item.</param>
        /// <param name="to">The target item.</param>
        /// <param name="force">If <see langword="true"/>, this function will always return <see langword="true"/>.</param>
        /// <returns>A value indicating if the target item (<paramref name="to"/>) needs to be rebuild or <see langword="true"/> if <paramref name="force"/> is set to <see langword="true"/>.</returns>
        public static bool NeedsBuild(string from, string to, bool force = false)
        {
            CLI.Debug.Log($">checking if '{from}' needs to be rebuild as '{to}'.");
            if (force)
            {
                CLI.Debug.Log($"skipped, '{nameof(force)}' is enabled.");
                return true;
            }

            if (!File.Exists(to))
            {
                CLI.Debug.Success($"<build needed, file at build target does not exist.");
                return true;
            }

            DateTime fromModDate = File.GetLastWriteTime(from);
            DateTime toModDate = File.GetLastWriteTime(to);

            if (fromModDate > toModDate)
            {
                CLI.Debug.Success($"rebuild pending.");
                return true;
            }
            else
            {
                CLI.Debug.Success($"no rebuild needed.");
                return false;
            }
        }

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
