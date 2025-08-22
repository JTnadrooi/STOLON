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
                new ConditionalNote("Gains 20% valloc when Deceit is selected.", i => i.Contains("deceit"), ConditionalNotePolarity.Positive),
                new ConditionalNote("Loses 50% valloc when more than 3 entities are selected.", i => i.Entries.Count > 3, ConditionalNotePolarity.Positive),
            ],
            [
                new ConditionalNote("If valloc is above 50%, gain the abilty to decide where the opponent places their marker. You will not be able to win in one of the 3 moves after.", i => i.GetVirtualAllocation("silo") > 50, ConditionalNotePolarity.Positive),
                new ConditionalNote("If valloc is above 20%, gain the abilty to remove oppenent markers from a row, must be normal gravity.", i => i.GetVirtualAllocation("silo") > 20, ConditionalNotePolarity.Positive),
            ], "Silo", "Silo 28SHA")
        { }
        public override int GetVirtualAllocation(EntitySelection info)
        {
            return new AllocationHelperChain(info, this)
                .ApplyMultiplier(info.Contains("deceit"), 1.2f)
                .ApplyMultiplier(info.Entries.Count > 3, 0.5f)
                .End();
        }

        public override Computer? Computer => null;
    }
}
