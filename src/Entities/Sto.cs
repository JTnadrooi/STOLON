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
    public class StoEntity : EntityBase
    {
        public override Computer Computer => null!;
        public override string? Description => "This also shoulden't be readable in the current verion.";

        public StoEntity() : base("sto", "Sto", "St")
        {

        }
    }
}