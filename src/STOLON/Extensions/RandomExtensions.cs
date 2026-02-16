using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Extensions
{
    public static class RandomExtensions
    {
        public static Vector2 GetRandomVector(this Random random, Vector2? max = null, Vector2? minSize = null)
        {
            max ??= new Vector2(float.MaxValue, float.MaxValue);
            minSize ??= Vector2.Zero;
            return new Vector2(random.Next((int)minSize.Value.X, (int)max.Value.X), random.Next((int)minSize.Value.Y, (int)max.Value.Y));
        }
    }
}
