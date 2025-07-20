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
        public SiloEntity() : base("silo", "Silo", "Sl", new EntityProfile("silo", new Point(245, 180)), "In the Etruscan language, plosive consonants had no contrastive voicing, so the Greek '' (Gamma) was adopted into the Etruscan alphabet to represent /k/. Already in the Western Greek alphabet, Gamma first took a '' form in Early Etruscan, then '' in Classical Etruscan. In Latin, it eventually took the 'c' form in Classical Latin. In the earliest Latin inscriptions, the letters 'c k q' were used to represent the sounds /k/ and // (which were not differentiated in writing). Of these, 'q' was used to represent /k/ or // before a rounded vowel, 'k' before 'a', and 'c' elsewhere.[3] During the 3rd century BC, a modified character was introduced for //, and 'c' itself was retained for /k/. The use of 'c' (and its variant 'g') replaced most usages of 'k' and 'q'. Hence, in the classical period and after, 'g' was treated as the equivalent of Greek gamma, and 'c' as the equivalent of kappa; this shows in the romanization of Greek words, as in '', '', and '' came into Latin as 'cadmvs', 'cyrvs' and 'phocis', respectively.", "Silo 28SHA")
        {
        }

        //public override int GetVirtualAllocation(int allocation, HashSet<Entity> entities)
        //{
        //    return allocation * (entities.Contains())
        //}

        public override Computer? Computer => null;
    }
}
