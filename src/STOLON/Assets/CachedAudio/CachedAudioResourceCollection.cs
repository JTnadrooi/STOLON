using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class CachedAudioResourceCollection : ResourceCollection<CachedAudio>
    {
        public CachedAudioResourceCollection() : base(new CachedAudioResourceLoader()) { }
    }
}
