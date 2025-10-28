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
            BuildHelper.Copy(src, dest);
        }

        public BuildItemInfo[] GetBuildItems() => BuildItemInfo.GetRelativeBuildItems(CLI.SourcePath! + @"STOLON\resources\Textures\", "*.png");
    }
}
