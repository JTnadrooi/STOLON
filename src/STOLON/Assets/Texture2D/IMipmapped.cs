namespace STOLON
{
    public interface IMipmapped
    {
        public IReadOnlyDictionary<int, Texture2D> Mipmaps { get; }
    }

    public static class MipmappedExtensions
    {
        public static Texture2D GetMipmap(this IMipmapped mipmappedObj, int res) => mipmappedObj.Mipmaps[res];

        /// <summary>
        /// Gets the highest available resolution mipmap.
        /// </summary>
        public static Texture2D GetHighestResolutionMipmap(this IMipmapped mipmappedObj)
            => mipmappedObj.GetMipmap(mipmappedObj.Mipmaps.Keys.Max());

        /// <summary>
        /// Gets the lowest available resolution mipmap.
        /// </summary>
        public static Texture2D GetLowestResolutionMipmap(this IMipmapped mipmappedObj)
            => mipmappedObj.GetMipmap(mipmappedObj.Mipmaps.Keys.Min());

        /// <summary>
        /// Attempts to get a mipmap of a given resolution. Returns <see langword="true"/> if found.
        /// </summary>
        public static bool TryGetMipmap(this IMipmapped mipmappedObj, int res, out Texture2D? texture)
            => mipmappedObj.Mipmaps.TryGetValue(res, out texture);

        public static bool HasMipmap(this IMipmapped mipmappedObj, int res)
            => mipmappedObj.Mipmaps.ContainsKey(res);
        public static bool Validate(this IMipmapped mipmappedObj, bool throwException = false)
        {
            if (mipmappedObj.Mipmaps.Any(kvp => kvp.Value.Width == kvp.Value.Height && kvp.Value.Width == kvp.Key))
                if (throwException) throw new Exception();
                else return true;
            return false;
        }
    }
}
