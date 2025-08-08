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
    public class SiloEntity : Entity
    {
        public SiloEntity() : base("silo", "Silo", "Sl", new EntityProfile("silo", new Point(245, 180)), 
            [
                new EntityNote("Gains a 20% boost in virtual allocation when Deceit is included in the selection.", i => i.IsSelected("deceit")),
            ], "Silo", "Silo 28SHA")
        {
        }

        public override int GetVirtualAllocation(int allocation, HashSet<Entity> entities)
        {
            return (int)(allocation * (entities.Any(e => e.Id == "deceit") ? 1.2f : 1f));
        }

        public override Computer? Computer => null;
    }
}
