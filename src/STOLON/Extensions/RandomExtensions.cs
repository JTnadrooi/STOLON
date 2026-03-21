using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Extensions
{
    public static class RandomExtensions
    {
        public static Vector2 NextVector2(this Random random, Vector2? max = null, Vector2? min = null)
        {
            max ??= new Vector2(float.MaxValue, float.MaxValue);
            min ??= new Vector2(float.MinValue, float.MinValue);
            return new Vector2(random.Next((int)min.Value.X, (int)max.Value.X), random.Next((int)min.Value.Y, (int)max.Value.Y));
        }
    }
}
