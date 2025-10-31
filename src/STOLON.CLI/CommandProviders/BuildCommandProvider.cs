using AsitLib;
using AsitLib.Stele;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics.Effects;
using STOLON.CLI.Build;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STOLON.CLI.Command;

namespace STOLON.CLI
{
    public class BuildCommandProvider : CommandProvider
    {
        public BuildCommandProvider() : base("build") { }

        [Command("Build content.", flags: CommandFlag.DevOnly)]
        public void _M(bool debug = false, bool force = false)
        {
            Effects(debug, force);
            Audio(force);
            Fonts(force);
            Textures(force);
        }

        [Command("Build effects.", id: "fx", flags: CommandFlag.DevOnly)]
        public void Effects(bool debug = false, bool force = false)
        {
            Builder.Build(new EffectsBuilder(debug), force);
        }

        [Command("Build audio.", flags: CommandFlag.DevOnly)]
        public void Audio(bool force = false)
        {
            Builder.Build(new AudioBuilder(), force);
        }

        [Command("Build fonts.", flags: CommandFlag.DevOnly)]
        public void Fonts(bool force = false)
        {
            Builder.Build(new FontsBuilder(), force);
        }

        [Command("Build textures.", flags: CommandFlag.DevOnly)]
        public void Textures(bool force = false)
        {
            Builder.Build(new TexturesBuilder(), force);
        }
    }
}
