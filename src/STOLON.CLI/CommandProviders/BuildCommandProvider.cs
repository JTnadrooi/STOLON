using AsitLib;
using AsitLib.Stele;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using STOLON.CLI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class BuildCommandProvider : CommandProvider
    {
        public BuildCommandProvider() : base("build")
        {

        }

        [Command("Build content.")]
        public void _M(bool debug = false)
        {
            Effects(debug);
        }

        [Command("Compile effects.", idOverride: "fx", needsDev: true)]
        public void Effects(bool debug = false)
        {
            string effectsSource = CLI.SourcePath! + @"STOLON\resources\Effects\";
            string[] rawEffects = Directory.GetFiles(effectsSource, "*.fx");

            CLI.Debug.Log($">building effects from: '{effectsSource}'.");
            CLI.Debug.Log($"found {rawEffects.Length} files.");
            Directory.CreateDirectory("Effects\\");

            for (int i = 0; i < rawEffects.Length; i++)
            {
                string effect = rawEffects[i];
                string target = Path.Combine(@".\Effects", Path.GetFileNameWithoutExtension(rawEffects[i]) + ".mgfx");

                if (!BuildHelper.NeedsBuild(effect, target)) continue;

                CLI.Debug.Log($">compiling '{effect}' as '{target}'.");

                using Process p = Process.Start("powershell.exe", $"dotnet tool run mgfxc {rawEffects[i]} {target} /Profile:OpenGL" + (debug ? " /Debug" : string.Empty));
                p.WaitForExit();

                CLI.Debug.Success();
            }

            CLI.Debug.Success();
        }
    }
}
