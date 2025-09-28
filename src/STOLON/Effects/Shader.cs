using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using MonoGame.Extended.ECS;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace STOLON
{
    public abstract class Shader
    {
        private bool _enabled = true;
        public virtual bool Enabled { get => _enabled; set => _enabled = value; }
        public abstract bool Virtual { get; }
        public abstract Effect Effect { get; }
        public virtual void UpdateResolution(Point newDesiredRes) { }
    }

    public class StolonReplaceColorShader : Shader
    {
        public override Effect Effect { get; }
        public override bool Virtual => true;
        public StolonReplaceColorShader()
        {
            Effect = STOLON.Instance.Content.Load<Effect>("Effects\\apply_palette");
            Effect.Parameters["dcolor1"].SetValue(Color.White.ToVector4());
            Effect.Parameters["color1"].SetValue(STOLON.Instance.Color1.ToVector4());
            Effect.Parameters["dcolor2"].SetValue(Color.Black.ToVector4());
            Effect.Parameters["color2"].SetValue(STOLON.Instance.Color2.ToVector4());
        }
        public override void UpdateResolution(Point newDesiredRes) { }
    }

    public class CRTShader : Shader
    {
        public override Effect Effect { get; }
        public override bool Virtual => false;
        public CRTShader()
        {
            Effect = STOLON.Instance.Content.Load<Effect>("Effects\\crt");
            Effect.Parameters["brightboost"].SetValue(0.92f);
            Effect.Parameters["textureSize"].SetValue(STOLON.Instance.DesiredDimensions.ToVector2());
            Effect.Parameters["outputSize"].SetValue(STOLON.Instance.DesiredDimensions.ToVector2());
        }
        public override void UpdateResolution(Point newDesiredRes)
        {
            Effect.Parameters["textureSize"].SetValue(newDesiredRes.ToVector2());
            Effect.Parameters["outputSize"].SetValue(newDesiredRes.ToVector2());
        }
    }
}
