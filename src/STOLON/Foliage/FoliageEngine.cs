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
        private readonly record struct FoliageTextureDrawInfo(Texture2D Texture, Vector2 Pos, bool DrawMirrored);

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
        private List<FoliageTextureDrawInfo> _cache;
        private List<Vector2> _pointCache;

        private Vector2 _point1;
        private Vector2 _point2;

        public Foliage(FoliageEngine engine, Vector2 p1, Vector2 p2)
        {
            _engine = engine;

            _point1 = p1;
            _point2 = p2;

            _count = (int)Vector2.Distance(p1, p2) / 25;
            _count = 2;

            _cache = new List<FoliageTextureDrawInfo>(_count);
            _pointCache = new List<Vector2>(_count);
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

            if (_point1.Y != _point2.Y) throw new InvalidOperationException("Points must have same Y value.");

            Console.WriteLine("update points for " + _seed);

            _cache.Clear();
            _pointCache.Clear();

            float lenght = Vector2.Distance(_point1, _point2);

            float minSpacingMod = (1f / _count) * 0.5f;

            float segmentLenghtMod = 1f / _count;

            HashSet<Texture2D> addedTextures = new HashSet<Texture2D>();
            float lastPlacedFarBoundEndAlongLine = -1; // 1d position of last placed along the line + half texture width. (NOT A MODIFIER)
            float overlapMod = .7f; // more = less overlap allowed. (max 1)

            for (int i = 0; i < _count; i++)
            {
                float jitter = Hash01(_seed * (i + 1)) * (segmentLenghtMod - minSpacingMod);
                float lerpAmount = i * segmentLenghtMod + jitter;

                Vector2 basePos = Vector2.Lerp(_point1, _point2, lerpAmount);
                _pointCache.Add(basePos);

                float distToLeft = Math.Abs(basePos.X - _point1.X);
                float distToRight = Math.Abs(basePos.X - _point2.X);
                int spaceToEnds = (int)Math.Min(distToLeft, distToRight) * 2;

                int spaceToPrevious = lastPlacedFarBoundEndAlongLine == -1 ? STOLON.VWidth : (int)((((basePos - _point1).X - lastPlacedFarBoundEndAlongLine)) / overlapMod);

                int maxSpace = Math.Min(spaceToEnds, spaceToPrevious * 2);

                Texture2D? texture = _engine.GetTexture(_seed, i, maxSpace, MaxReach, out Vector2 offset);

                if (texture is null)
                    continue;

                bool drawMirrored = unchecked((_seed * i) % 2) == 0;
                Vector2 drawPos = basePos + offset;

                NumberHelper.OnPixel(ref drawPos);

                //if (!addedTextures.Contains(texture))
                {
                    _cache.Add(new FoliageTextureDrawInfo(texture, drawPos, drawMirrored));
                    addedTextures.Add(texture);
                    lastPlacedFarBoundEndAlongLine = (basePos - _point1).X + texture.Width * 0.5f;
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
            drawingContext.DrawPoint(_point1, Color.Blue, 6);
            drawingContext.DrawPoint(_point2, Color.Blue, 6);

            for (int i = 0; i < _pointCache.Count; i++)
            {
                drawingContext.DrawPoint(_pointCache[i], Color.Red, 6);
            }

            for (int i = 0; i < _cache.Count; i++)
            {
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
        private readonly record struct FoliageTexture(Texture2D Texture, Sides SupportedSides, int BaseOffset, int BaseStart, int BaseLenght)
        {
            public FoliageTexture(Texture2D texture) : this(default!, default, default, default, default) // foliage1-t;1;1;1
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
        private readonly FoliageTexture[] _foliageTextures;

        private const int MaxFoliages = 100;

        public FoliageEngine(ITexture2DCollection textures, Random random)
        {
            _textures = textures;
            _random = random;

            _foliages = new List<WeakReference<Foliage>>(MaxFoliages);

            List<Texture2D> foliageTextures = new List<Texture2D>();

            foliageTextures.AddRange(textures.Resources.Where(kvp => kvp.Key.Contains("foliage")).Select(kvp => kvp.Value));

            _foliageTextures = foliageTextures.Select(t => new FoliageTexture(t)).ToArray();
        }

        internal int Register(Foliage foliage)
        {
            _foliages.Add(new WeakReference<Foliage>(foliage));

            int seed = _random.Next();

            unchecked
            {
                seed *= foliage.GetHashCode();
            }

            return Math.Abs(seed);
        }

        internal void Deregister(Foliage foliage)
        {
            _foliages.RemoveAll(wr => wr.TryGetTarget(out Foliage? f) && f == foliage);
        }

        internal Texture2D? GetTexture(int seed, int index, int sizeX, int sizeY, out Vector2 offset)
        {
            int foliageTextureIndex;
            FoliageTexture[] availableFoliageTextures = _foliageTextures.Where(f => f.Texture.Bounds.Width < sizeX && f.Texture.Bounds.Height < sizeY).ToArray(); // sloww.

            //availableFoliageTextures = _foliageTextures
            //    .Where(f => f.Texture.Bounds.Width < sizeX && f.Texture.Bounds.Height < sizeY)
            //    .ToArray();
            //if (availableFoliageTextures.Length == 0)
            //    availableFoliageTextures = Array.Empty<FoliageTexture>();
            //else
            //    availableFoliageTextures = availableFoliageTextures
            //        .OrderByDescending(f => f.Texture.Bounds.Width)
            //        .Take(1)
            //        .ToArray();

            if (availableFoliageTextures.Length == 0)
            {
                Console.WriteLine("failed for (" + sizeX + ", " + sizeY + ")");
                offset = default;
                return null;
            }

            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ index;
                hash = hash * 397 ^ (sizeX << 16) | (sizeY & 0xFFFF);
                hash = Math.Abs(hash);
                foliageTextureIndex = hash % availableFoliageTextures.Length;
            }

            FoliageTexture result = availableFoliageTextures[foliageTextureIndex];

            offset = new Vector2(-result.Texture.Width / 2, -result.Texture.Height + availableFoliageTextures[foliageTextureIndex].BaseOffset);

            Console.WriteLine(result.Texture.Name + " for (" + sizeX + ", " + sizeY + ")");

            return result.Texture;
        }

        public void Update(int elapsedMilliseconds) // doesnt do anything yet as i dont have a wind shader..... (also doesnt get called!!)
        {

        }
    }
}
