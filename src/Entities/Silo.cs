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
                new EntityNote("Gains 20% allocation when Deceit is included in the selection.", i => i.IsSelected("deceit"), false),
                new EntityNote("Loses 50% allocation when more than 3 entities are included in the selection.", i => i.Entries.Count > 3, true),
            ], "Silo", "Silo 28SHA")
        {
        }

        public override int GetVirtualAllocation(SelectionInfo info)
        {
            //return (int)(info.Entries[this.Id].Allocation * (info.IsSelected("deceit") ? 1.2f : 1f));
            return AllocationHelpers.Start(info, this).ApplyMultiplier(info.IsSelected("deceit"), 1.2f).End();
        }

        public override Computer? Computer => null;
    }
}
