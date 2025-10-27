using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    public class FontsBuilder : IBuilder
    {
        public FontsBuilder() { }

        public void PreBuild()
        {
            Directory.CreateDirectory("Fonts\\");
        }

        public void Build(string src, string dest)
        {
            File.Copy(src, dest, true);
        }

        public BuildItemInfo[] GetBuildItems()
            => Directory.GetFiles(CLI.SourcePath! + @"STOLON\resources\Fonts\", "*", SearchOption.AllDirectories)
            .Select(f => new BuildItemInfo(f, Path.Combine(@".\Fonts", Path.GetFileName(f))))
            .ToArray();
    }
}
