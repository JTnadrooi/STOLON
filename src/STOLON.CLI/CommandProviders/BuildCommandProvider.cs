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
    public class BuildCommandProvider : CommandGroup
    {
        public BuildCommandProvider() : base("build", CLI.InfoFactory, nameOfMainMethod: nameof(Main)) { }

        [FlaggedCommand("Builds content.", Flags = CommandFlags.DevOnly, PassingPolicies = OptionPassingPolicies.Named)]
        public void Main(bool debug = false, bool force = false)
        {
            Effects(debug, force);
            Audio(force);
            Fonts(force);
            Textures(force);
        }

        [FlaggedCommand("Builds effects.", Id = "fx", Flags = CommandFlags.DevOnly)]
        public void Effects(bool debug = false, bool force = false)
        {
            Builder.Build(new EffectsBuilder(debug), force);
        }

        [FlaggedCommand("Builds audio.", Flags = CommandFlags.DevOnly)]
        public void Audio(bool force = false)
        {
            Builder.Build(new AudioBuilder(), force);
        }

        [FlaggedCommand("Builds fonts.", Flags = CommandFlags.DevOnly)]
        public void Fonts(bool force = false)
        {
            Builder.Build(new FontsBuilder(), force);
        }

        [FlaggedCommand("Builds textures.", Flags = CommandFlags.DevOnly)]
        public void Textures(bool force = false)
        {
            Builder.Build(new TexturesBuilder(), force);
        }
    }
}
