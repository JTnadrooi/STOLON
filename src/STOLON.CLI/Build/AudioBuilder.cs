using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Build
{
    public class AudioBuilder : IBuilder
    {
        public AudioBuilder() { }

        public void PreBuild()
        {
            Directory.CreateDirectory("Audio\\");
        }

        public void Build(string src, string dest)
        {
            File.Copy(src, dest, true);
        }

        public BuildItemInfo[] GetBuildItems()
            => Directory.GetFiles(CLI.SourcePath! + @"STOLON\resources\Audio\", "*.wav")
            .Select(f => new BuildItemInfo(f, Path.Combine(@".\Audio", Path.GetFileName(f))))
            .ToArray();
    }
}
