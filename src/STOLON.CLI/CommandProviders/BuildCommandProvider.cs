using AsitLib;
using AsitLib.Stele;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public void _M()
        {
            Effects();
        }

        [Command("Compile effects.", idOverride: "fx", needsDev: true)]
        public void Effects()
        {
            string effectsSource = CLI.SourcePath! + @"\STOLON\Content\Effects";
            Directory.CreateDirectory("Effects\\");
            string[] effects = Directory.GetFiles(effectsSource, "*.fx");
            CLI.Debug.Log($">building effects from: '{effectsSource}'.");

            for (int i = 0; i < effects.Length; i++)
            {
                string effect = "Effects\\" + effects[i][(effectsSource.Length + 1)..];
                string target = "Effects\\" + effects[i][(effectsSource.Length + 1)..^(".fx".Length)] + ".mgfx";
                CLI.Debug.Log($"found '{effect}', compiling as '{target}'.");

                using Process p = Process.Start("powershell.exe", $"dotnet tool run mgfxc {effects[i]} {target} /Profile:OpenGL");
                p.WaitForExit();
            }

            CLI.Debug.Success();
        }
    }
}
