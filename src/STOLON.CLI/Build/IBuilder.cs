using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    public class BuildItemInfo
    {
        public string Source { get; }
        public string Destination { get; }
        public BuildItemInfo(string src, string dest)
        {
            Source = src;
            Destination = dest;
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
