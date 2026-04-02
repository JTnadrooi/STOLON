namespace STOLON
{
    public enum EntityDrawMode
    {
        None = 0,
        Menu = 1,
        WithBackground = 2,
    }
    public class EntityProfile : IMipmapped
    {
        public Texture2D Texture512 => Mipmaps[512];
        public Texture2D Texture256 => this.TryGetMipmap(256, out Texture2D? t) ? t! : throw new Exception();
        public Texture2D Texture128 => this.TryGetMipmap(128, out Texture2D? t) ? t! : throw new Exception();

        public IReadOnlyDictionary<int, Texture2D> Mipmaps => mipmaps;
        public Point Focus { get => _focus; set => _focus = value; }
        public Point MenuOffset { get => _menuOffset; set => _menuOffset = value; }

        private Dictionary<int, Texture2D> mipmaps;
        private Point _focus;
        private Point _menuOffset;

        public EntityProfile(string entityName, ITexture2DCollection textures, Point? focus = null, Point? menuOffset = null) : this(
            textures.TryGetValue($"Entities\\{entityName}\\{entityName}-512", out Texture2D? val512) ? val512 : throw new Exception(),
            textures.TryGetValue($"Entities\\{entityName}\\{entityName}-256", out Texture2D? val256) ? val256 : null,
            textures.TryGetValue($"Entities\\{entityName}\\{entityName}-128", out Texture2D? val128) ? val128 : null,
            focus)
        { }

        public EntityProfile(Texture2D t512, Texture2D? t256 = null, Texture2D? t128 = null, Point? focus = null, Point? menuOffset = null)
        {
            mipmaps = new Dictionary<int, Texture2D>();
            mipmaps[512] = t512;
            if (t256 != null) mipmaps[256] = t256;
            if (t128 != null) mipmaps[128] = t128;
            _focus = focus ?? Centering.GetCenter(t512).ToPoint();
            _menuOffset = menuOffset ?? Point.Zero;
        }

        public static EntityProfile GetDebug(Texture2D? t512, Texture2D? t256 = null, Texture2D? t128 = null, ITexture2DCollection? textures = null, Point? focus = null, Point? menuOffset = null)
            => new EntityProfile(t512 ?? textures[$"Debug\\temp-512"] ?? throw new InvalidOperationException(), t256, t128, focus, menuOffset);
        public static EntityProfile GetDebug(string entityName, ITexture2DCollection textures, Point? focus = null, Point? menuOffset = null)
            => new EntityProfile(
                textures.TryGetValue($"Entities\\{entityName}\\{entityName}-512", out Texture2D? val512) ? val512 : textures[$"Debug\\temp-512"],
                textures.TryGetValue($"Entities\\{entityName}\\{entityName}-256", out Texture2D? val256) ? val256 : null,
                textures.TryGetValue($"Entities\\{entityName}\\{entityName}-128", out Texture2D? val128) ? val128 : null,
            focus);
    }
}
