using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public static float GetBrightness(this Color color, bool cheap = true)
        {
            float r1 = color.R / (float)byte.MaxValue;
            float g1 = color.G / (float)byte.MaxValue;
            float b1 = color.B / (float)byte.MaxValue;
            if (cheap) return 0.2126f * r1 + 0.7152f * g1 + 0.0722f * b1;
            else return MathF.Sqrt(0.299f * MathF.Pow(r1, 2f) + 0.587f * MathF.Pow(g1, 2) + 0.114f * MathF.Pow(g1, 2));
        }
    }
}
