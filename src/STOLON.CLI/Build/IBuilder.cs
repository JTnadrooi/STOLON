using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    public sealed class BuildItemInfo
    {
        public string Source { get; }
        public string Destination { get; }

        public BuildItemInfo(string src, string dest)
        {
            Source = src;
            Destination = dest;
        }

        public static BuildItemInfo[] GetRelativeBuildItems(string src, string pattern, Func<string, string>? selector = null)
        {
            Func<string, string> sel = selector ?? (f => f);
            return Directory.GetFiles(src, pattern, SearchOption.AllDirectories)
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

    public interface IBuilder
    {
        public BuildItemInfo[] GetBuildItems();
        public void PreBuild() { }
        public void PostBuild() { }
        public void Build(string src, string dest);
    }
}
