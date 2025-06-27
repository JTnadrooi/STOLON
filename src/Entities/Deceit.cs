using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using AsitLib;

using Math = System.Math;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Input;
using System.Xml.Linq;
using Microsoft.Xna.Framework;



namespace STOLON
{
    public class DeceitEntity : Entity
    {
        public DeceitEntity() : base("deceit", "Deceit", "Dc", new EntityProfile("deceit", null, new Point(-130, -40)), "Yeah", "Tomboyish elf #82")
        {
        }

        public override Computer? Computer => null;
    }
}
