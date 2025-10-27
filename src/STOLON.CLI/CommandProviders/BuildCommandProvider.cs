using AsitLib;
using AsitLib.Stele;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics.Effects;
using STOLON.CLI.Build;
using STOLON.CLI.Helpers;
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

        [Command("Build content.")]
        public void _M(bool debug = false, bool force = false)
        {
            Effects(debug, force);
            Audio(force);
            Fonts(force);
            Textures(force);
        }

        [Command("Compile effects.", idOverride: "fx", needsDev: true)]
        public void Effects(bool debug = false, bool force = false)
        {
            Builder.Build(new EffectsBuilder(debug), force);
        }

        [Command("Compile audio.", needsDev: true)]
        public void Audio(bool force = false)
        {
            Builder.Build(new AudioBuilder(), force);
        }

        [Command("Compile fonts.", needsDev: true)]
        public void Fonts(bool force = false)
        {
            Builder.Build(new FontsBuilder(), force);
        }

        [Command("Compile textures.", needsDev: true)]
        public void Textures(bool force = false)
        {
            Builder.Build(new TexturesBuilder(), force);
        }
    }
}
