using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class EffectResourceCollection : ResourceCollection<Effect>
    {
        public EffectResourceCollection() : base(new EffectResourceLoader()) { }
    }
}
