using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    public class TexturesBuilder : IBuilder
    {
        public TexturesBuilder() { }

        public void PreBuild()
        {
            Directory.CreateDirectory("Textures\\");
        }

        public void Build(string src, string dest)
        {
            File.Copy(src, dest, true);
        }

        public BuildItemInfo[] GetBuildItems()
            => Directory.GetFiles(CLI.SourcePath! + @"STOLON\resources\Textures\", "*.png", SearchOption.AllDirectories)
            .Select(f => new BuildItemInfo(f, Path.Combine(@".\Textures", Path.GetFileName(f))))
            .ToArray();
    }
}
