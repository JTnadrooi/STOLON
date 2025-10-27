using System.Diagnostics;

namespace STOLON.CLI.Build
{
    public class EffectsBuilder : IBuilder
    {
        private readonly bool _debug;

        public EffectsBuilder(bool debug)
        {
            _debug = debug;
        }

        public void PreBuild()
        {
            Directory.CreateDirectory("Effects\\");
        }

        public void Build(string src, string dest)
        {
            using Process p = Process.Start("powershell.exe", $"dotnet tool run mgfxc {src} {dest} /Profile:OpenGL" + (_debug ? " /Debug" : string.Empty));
            p.WaitForExit();
        }

        public BuildItemInfo[] GetBuildItems()
            => Directory.GetFiles(CLI.SourcePath! + @"STOLON\resources\Effects\", "*.fx", SearchOption.AllDirectories)
            .Select(f => new BuildItemInfo(f, Path.Combine(@".\Effects", Path.GetFileNameWithoutExtension(f) + ".mgfx")))
            .ToArray();
    }
}
