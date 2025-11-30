using AsitLib;
using AsitLib.CommandLine;
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

namespace STOLON.CLI
{
    public class BuildCommandProvider : CommandProvider
    {
        public BuildCommandProvider() : base("build") { }

        [SLCommand("Builds content.", Flags = CommandFlags.DevOnly, IsMain = true)]
        public void Main(bool debug = false, bool force = false)
        {
            Effects(debug, force);
            Audio(force);
            Fonts(force);
            Textures(force);
        }

        [SLCommand("Builds effects.", Id = "fx", Flags = CommandFlags.DevOnly)]
        public void Effects(bool debug = false, bool force = false)
        {
            Builder.Build(new EffectsBuilder(debug), force);
        }

        [SLCommand("Builds audio.", Flags = CommandFlags.DevOnly)]
        public void Audio(bool force = false)
        {
            Builder.Build(new AudioBuilder(), force);
        }

        [SLCommand("Builds fonts.", Flags = CommandFlags.DevOnly)]
        public void Fonts(bool force = false)
        {
            Builder.Build(new FontsBuilder(), force);
        }

        [SLCommand("Builds textures.", Flags = CommandFlags.DevOnly)]
        public void Textures(bool force = false)
        {
            Builder.Build(new TexturesBuilder(), force);
        }
    }
}
