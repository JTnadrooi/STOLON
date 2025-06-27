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
    public class NorthEntity : Entity
    {
        public NorthEntity() : base("north", "North", "Nth", EntityProfile.GetDebug("north"), string.Empty, "Noria-aeth")
        {
        }

        public override Computer? Computer => null;
    }
}
