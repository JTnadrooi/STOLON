using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class Foliage : IDrawable, IDisposable
    {
        private readonly record struct FoliageAssetDrawInfo(Texture2D Texture, Vector2 Pos, bool DrawMirrored);

        private readonly FoliageEngine _engine;

        private readonly int _seed;

        public Vector2 Point1
        {
            get => _point1;
            set
            {
                _point1 = value;
                UpdatePoints();
            }
        }

        public Vector2 Point2
        {
            get => _point2;
            set
            {
                _point2 = value;
                UpdatePoints();
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of distance this foliage can reach from the <see cref="Line"/>. 
        /// </summary>
        public int MaxReach { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="global::STOLON.Line"/> from <see cref="Point1"/> to <see cref="Point2"/>.
        /// </summary>
        public Line Line
        {
            get => new Line(_point1, _point2);
            set
            {
                SetPoints(value.Start, value.End);
            }
        }

        private int _count;
        private bool _isDisposed;
        private List<FoliageAssetDrawInfo> _cache;

        private Vector2 _point1;
        private Vector2 _point2;

        public Foliage(FoliageEngine engine, Vector2 p1, Vector2 p2)
        {
            _engine = engine;

            _point1 = p1;
            _point2 = p2;

            _count = 5;

            _cache = new List<FoliageAssetDrawInfo>(_count);
            _seed = engine.Register(this);

            MaxReach = int.MaxValue;

            UpdatePoints(); // _count gets set here
        }

        private void UpdatePoints()
        {
            static float Hash01(int x)
            {
                unchecked
                {
                    x ^= x << 13;
                    x ^= x >> 17;
                    x ^= x << 5;
                    return (x & 0x7fffffff) / (float)int.MaxValue;
                }
            }

            Console.WriteLine("update points for " + _seed);

            _cache.Clear();

            float lenght = Vector2.Distance(_point1, _point2);

            float minSpacingMod = (1f / _count) * 0.5f;

            Console.WriteLine(minSpacingMod);

            float segmentLenghtMod = 1f / _count;

            HashSet<Texture2D> addedTextures = new HashSet<Texture2D>();
            float lastPlacedRight = float.NegativeInfinity;
            float overlapMod = 1f; // more = less overlap allowed. (max 1)

            for (int i = 0; i < _count; i++)
            {
                float jitter = Hash01(_seed * (i + 1)) * (segmentLenghtMod - minSpacingMod);
                float lerpAmount = i * segmentLenghtMod + jitter;

                Vector2 basePos = Vector2.Lerp(_point1, _point2, lerpAmount);

                float distToLeft = Math.Abs(basePos.X - _point1.X);
                float distToRight = Math.Abs(basePos.X - _point2.X);
                int spaceToEnds = (int)Math.Min(distToLeft, distToRight) * 2;

                int spaceToPrevious = float.IsNegativeInfinity(lastPlacedRight) ? int.MaxValue : (int)((basePos.X - lastPlacedRight) * 2 / overlapMod);

                int maxSpace = Math.Min(spaceToEnds, spaceToPrevious);

                Texture2D? texture = _engine.GetFoliageAsset(_seed, i, maxSpace, MaxReach, out Vector2 offset);

                if (texture is null) continue;

                bool drawMirrored = unchecked((_seed * i) % 2) == 0;
                Vector2 pos = basePos + offset;

                NumberHelper.OnPixel(ref pos);

                if (!addedTextures.Contains(texture))
                {
                    _cache.Add(new FoliageAssetDrawInfo(texture, pos, drawMirrored));
                    addedTextures.Add(texture);
                    lastPlacedRight = pos.X + texture.Width * 0.5f;
                }
            }
        }

        public void SetPoints(Vector2 p1, Vector2 p2)
        {
            _point1 = p1;
            _point2 = p2;

            UpdatePoints();
        }

        public void Draw(DrawingContext drawingContext)
        {
            //for (int i = 0; i < Points.Length; i++)
            //{
            //    drawingContext.DrawPoint(Points[i], Color.Aqua, 5);
            //}

            for (int i = 0; i < _cache.Count; i++)
            {
                //drawingContext.DrawPoint(_cache[i].Pos, Color.Red, 5);
                drawingContext.Draw(_cache[i].Texture, _cache[i].Pos, effects: _cache[i].DrawMirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
        }

        public static Foliage FromRectangle(FoliageEngine engine, Rectangle r)
        {
            return new Foliage(engine, new Vector2(r.Left, r.Bottom), new Vector2(r.Right, r.Bottom)); // inverted y.
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _engine.Deregister(this);

            _isDisposed = true;
        }
    }

    [Dependency(ServiceLifetime.Singleton)]
    public sealed class FoliageEngine : IUpdatable
    {
        private readonly record struct FoliageAsset(Texture2D Texture, Sides SupportedSides, int BaseOffset, int BaseStart, int BaseLenght)
        {
            public FoliageAsset(Texture2D texture) : this(default!, default, default, default, default) // foliage1-t;1;1;1
            {
                Texture = texture;

                const string foliagePrefix = "foliage";

                string textureName = Path.GetFileNameWithoutExtension(texture.Name);

                Debug.Assert(textureName.StartsWith(foliagePrefix));

                string metadataStr = textureName[foliagePrefix.Length..].Split("-").Last();
                string[] parts = metadataStr.Split(';');

                SupportedSides = parts[0] switch
                {
                    "l" => Sides.Left,
                    "t" => Sides.Top,
                    "r" => Sides.Right,
                    "b" => Sides.Bottom,
                    _ => throw new Exception()
                };

                BaseOffset = int.Parse(parts[1]);

                int baseStart = int.Parse(parts[2]);

                int baseLenght = int.Parse(parts[3]);
                baseLenght = baseLenght <= 0 ? texture.Width : baseLenght;

                BaseStart = baseStart;
                BaseLenght = baseLenght;
            }
        }

        private readonly ITexture2DCollection _textures;
        private readonly Random _random;

        private readonly List<WeakReference<Foliage>> _foliages; // its a word.
        private readonly FoliageAsset[] _foliageAssets;

        private const int MaxFoliages = 100;

        public FoliageEngine(ITexture2DCollection textures, Random random)
        {
            _textures = textures;
            _random = random;

            _foliages = new List<WeakReference<Foliage>>(MaxFoliages);

            List<Texture2D> foliageTextures = new List<Texture2D>();

            foliageTextures.AddRange(textures.Resources.Where(kvp => kvp.Key.Contains("foliage")).Select(kvp => kvp.Value));

            _foliageAssets = foliageTextures.Select(t => new FoliageAsset(t)).ToArray();
        }

        internal int Register(Foliage foliage)
        {
            _foliages.Add(new WeakReference<Foliage>(foliage));

            int seed = _random.Next();

            unchecked
            {
                seed *= foliage.GetHashCode();
                seed = seed < 0 ? seed / 2 : seed;
                seed = Math.Abs(seed); // just to be sure.
            }

            return Math.Abs(seed);
        }

        internal void Deregister(Foliage foliage)
        {
            _foliages.RemoveAll(wr => wr.TryGetTarget(out Foliage? f) && f == foliage);
        }

        internal Texture2D? GetFoliageAsset(int seed, int index, int sizeX, int sizeY, out Vector2 offset)
        {
            int foliageAssetIndex;
            //FoliageAsset[] availibleFoliageAssets = _foliageAssets.Where(f => (f.Group & group) != 0).ToArray(); // sloww.
            FoliageAsset[] availibleFoliageAssets = _foliageAssets.Where(f => f.Texture.Bounds.Width < sizeX && f.Texture.Bounds.Height < sizeY).ToArray(); // sloww.

            if (availibleFoliageAssets.Length == 0)
            {
                offset = default;
                return null;
            }

            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ index;
                hash = hash * 397 ^ (sizeX << 16) | (sizeY & 0xFFFF);
                hash = Math.Abs(hash);
                foliageAssetIndex = hash % availibleFoliageAssets.Length;
            }

            FoliageAsset result = availibleFoliageAssets[foliageAssetIndex];

            offset = new Vector2(-result.Texture.Width / 2, -result.Texture.Height + availibleFoliageAssets[foliageAssetIndex].BaseOffset);

            Console.WriteLine(result.Texture.Name + " for (" + sizeX + ", " + sizeY + ")");

            return result.Texture;
        }

        public void Update(int elapsedMilliseconds) // doesnt do anything yet as i dont have a wind shader..... (also doesnt get called!!)
        {

        }
    }
}
