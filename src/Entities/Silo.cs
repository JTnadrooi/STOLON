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
                new ConditionalNote("Gains 20% allocation when Deceit is included in the selection.", i => i.IsSelected("deceit"), ConditionalNoteDomain.Allocation, ConditionalNotePolarity.Positive),
                new ConditionalNote("Loses 50% allocation when more than 3 entities are included in the selection.", i => i.Entries.Count > 3, ConditionalNoteDomain.Allocation, ConditionalNotePolarity.Positive),
            ], "Silo", "Silo 28SHA")
        {
        }

        public override int GetVirtualAllocation(SelectionInfo info)
        {
            return new AllocationHelperChain(info, this)
                .ApplyMultiplier(info.IsSelected("deceit"), 1.2f)
                .ApplyMultiplier(info.Entries.Count > 3, 0.5f)
                .End();
        }

        public override Computer? Computer => null;
    }
}
