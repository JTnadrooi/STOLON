using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public static class Texture2DExtensions
    {
        public static Texture2D ReplaceColor(this Texture2D texture, Color color1, Color color2) => ReplaceColor(texture, [color1], color2);
        public static Texture2D ReplaceColor(this Texture2D texture, Color[] color1s, Color color2)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                if (color1s.Any(c => c == data[i])) data[i] = color2;
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D SetAllColor(this Texture2D texture, Color color)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++) data[i] = data[i] == Color.Transparent ? Color.Transparent : color;
            texture.SetData(data);
            return texture;
        }

        public static bool SamePrintAs(this Texture2D firstTexture, Texture2D secondTexture) => firstTexture.Bounds.SamePrintAs(secondTexture.Bounds);
    }
}
