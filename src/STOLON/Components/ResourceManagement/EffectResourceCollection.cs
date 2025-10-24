using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class EffectResourceLoader : SequentialResourceLoader<Effect>
    {
        public override string[] GetItems() => Directory.GetFiles("Effects", "*.mgfx", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Effects\\".Length..^".mgfx".Length];

        public override Effect LoadItem(string item)
        {
            return new Effect(STOLON.Instance.GraphicsDevice, File.ReadAllBytes(item))
            {
                Name = item
            };
        }
    }
    public class EffectResourceCollection : ResourceCollection<Effect>
    {
        public EffectResourceCollection() : base(new EffectResourceLoader()) { }
    }
}
