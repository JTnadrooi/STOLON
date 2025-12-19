using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    /// <summary>
    /// Represents a single needing to be build item.
    /// </summary>
    public sealed class BuildItemInfo
    {
        /// <summary>
        /// Gets the source of the item to get passed to the <see cref="IBuilder.Build(string, string)"/> method.
        /// </summary>
        public string Source { get; }
        /// <summary>
        /// Gets the destination of the item to get passed to the <see cref="IBuilder.Build(string, string)"/> method.
        /// </summary>
        public string Destination { get; }

        public BuildItemInfo(string src, string dest)
        {
            Source = src;
            Destination = dest;
        }

        /// <summary>
        /// Gets a value indicating if this <see cref="BuildItemInfo"/> needs to be rebuild.
        /// </summary>
        /// <param name="force">If <see langword="true"/>, this function will always return <see langword="true"/>.</param>
        /// <returns>A value indicating if the target item (<paramref name="Destination"/>) needs to be rebuild or <see langword="true"/> if <paramref name="force"/> is set to <see langword="true"/>.</returns>
        public bool NeedsBuild(bool force)
        {
            CLI.Logger.Log($">checking if '{Source}' needs to be rebuild as '{Destination}'.");
            if (force)
            {
                CLI.Logger.Log($"<skipped, '{nameof(force)}' is enabled.");
                return true;
            }

            if (!File.Exists(Destination))
            {
                CLI.Logger.Success($"<build needed, file at build target does not exist.");
                return true;
            }

            DateTime fromModDate = File.GetLastWriteTime(Source);
            DateTime toModDate = File.GetLastWriteTime(Destination);

            if (fromModDate > toModDate)
            {
                CLI.Logger.Success($"rebuild pending.");
                return true;
            }
            else
            {
                CLI.Logger.Success($"no rebuild needed.");
                return false;
            }
        }

        public static BuildItemInfo[] GetRelativeBuildItems(string src, string searchPattern, Func<string, string>? selector = null)
        {
            Func<string, string> sel = selector ?? (f => f);
            return Directory.GetFiles(src, searchPattern, SearchOption.AllDirectories)
                .Select(f => new BuildItemInfo(f, sel.Invoke(Path.Combine(new DirectoryInfo(src).Name, Path.GetRelativePath(src, f)))))
                .ToArray();
        }

        public static BuildItemInfo[] GetRelativeBuildItems(string src, Func<string, bool>? predicate = null, Func<string, string>? selector = null)
        {
            Func<string, string> sel = selector ?? (f => f);
            Func<string, bool> pred = predicate ?? (f => true);
            return Directory.GetFiles(src, "*", SearchOption.AllDirectories).Where(pred)
                .Select(f => new BuildItemInfo(f, sel.Invoke(Path.Combine(new DirectoryInfo(src).Name, Path.GetRelativePath(src, f)))))
                .ToArray();
        }
    }

    /// <summary>
    /// Represents an assets builder.
    /// </summary>
    public interface IBuilder
    {
        /// <summary>
        /// Gets the items that have to be build. Do not check if an item needs to be build. That is done by the <see cref="BuildHelper.NeedsBuild(string, string, bool)"/> function.
        /// </summary>
        /// <returns>An array of <see cref="BuildItemInfo"/> objects representing both the source of the item and the destination.</returns>
        public BuildItemInfo[] GetBuildItems();

        /// <summary>
        /// Gets called before the first <see cref="Build(string, string)"/> call.
        /// </summary>
        public void PreBuild() { }

        /// <summary>
        /// Gets called after the last <see cref="Build(string, string)"/> call.
        /// </summary>
        public void PostBuild() { }

        /// <summary>
        /// Builds a single item.
        /// </summary>
        /// <param name="src">The item source. See <see cref="BuildItemInfo.Source"/>.</param>
        /// <param name="dest">The item destination. See <see cref="BuildItemInfo.Destination"/>.</param>
        public void Build(string src, string dest);
    }
}
