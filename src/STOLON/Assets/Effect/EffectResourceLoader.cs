
namespace STOLON
{
    [Dependency(ServiceLifetime.Transient)]
    public class EffectResourceLoader : SequentialResourceLoader<Effect>
    {
        public EffectResourceLoader(IRichLogger logger) : base(logger)
        {
        }

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
}
