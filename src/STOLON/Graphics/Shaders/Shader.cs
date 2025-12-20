namespace STOLON
{
    public abstract class Shader
    {
        public bool IsEnabled { get; set; }
        public bool IsVirtual { get; }
        public Effect Effect { get; }
        public Shader(Effect effect, bool isVirtual = true, bool isEnabled = true)
        {
            Effect = effect;
            IsVirtual = isVirtual;
            IsEnabled = isEnabled;
        }

        public virtual void UpdateResolution(Point newDesiredRes) { }
    }

    public class StolonReplaceColorShader : Shader
    {
        public StolonReplaceColorShader() : base(STOLON.Effects["apply_palette"], true)
        {
            Effect.Parameters["dcolor1"].SetValue(Color.White.ToVector4());
            Effect.Parameters["color1"].SetValue(STOLON.Instance.Color1.ToVector4());
            Effect.Parameters["dcolor2"].SetValue(Color.Black.ToVector4());
            Effect.Parameters["color2"].SetValue(STOLON.Instance.Color2.ToVector4());
        }
    }

    public class CRTShader : Shader
    {
        public CRTShader() : base(STOLON.Effects["crt"], false)
        {
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
