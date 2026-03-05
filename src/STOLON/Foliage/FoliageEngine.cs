using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    internal readonly struct FoliageTexture
    {
        public readonly Texture2D Texture;
        public readonly Sides SupportedSides;
        public readonly int BaseX;
        public readonly int BaseY;
        public readonly int BaseLength;
        public readonly bool IsCorner;
        public readonly CornerType? CornerType;

        private const string FoliageCornerPrefix = "foliage_c";
        private const string FoliagePrefix = "foliage";

        public FoliageTexture(Texture2D texture)
        {
            Texture = texture;

            string textureName = Path.GetFileNameWithoutExtension(texture.Name);
            IsCorner = textureName.StartsWith(FoliageCornerPrefix);

            if (!textureName.StartsWith(FoliagePrefix))
                throw new InvalidResourceException($"Foliage texture '{texture.Name}' does not start with '{FoliagePrefix}'.");

            string metadataStr = textureName[(IsCorner ? FoliageCornerPrefix : FoliagePrefix).Length..].Split("-").Last();
            string[] parts = metadataStr.Split(';');

            if (parts.Length != 3 && parts.Length != 4)
                throw new InvalidResourceException($"Invalid foliage '{texture.Name}' with {parts.Length} metadata parts.");

            SupportedSides = IsCorner ? Sides.Top : parts[0] switch
            {
                "t" => Sides.Top,
                _ => throw new InvalidResourceException($"Invalid side specification '{parts[0]}' for texture '{texture.Name}'.")
            };

            if (IsCorner)
                CornerType = parts[0] switch
                {
                    "t" => global::STOLON.CornerType.Top,
                    _ => throw new InvalidResourceException($"Invalid corner type '{parts[0]}' for texture '{texture.Name}'.")
                };
            else
                CornerType = null;

            if (!int.TryParse(parts[1], out BaseX))
                throw new InvalidResourceException($"Invalid BaseX value '{parts[1]}' for texture '{texture.Name}'.");

            if (!int.TryParse(parts[2], out BaseY))
                throw new InvalidResourceException($"Invalid BaseY value '{parts[2]}' for texture '{texture.Name}'.");

            int defaultBaseLength = texture.Width - BaseX;

            if (parts.Length == 4)
            {
                if (int.TryParse(parts[3], out int parsedLength))
                    BaseLength = parsedLength <= 0 ? defaultBaseLength : parsedLength;
                else
                    throw new InvalidResourceException($"Invalid BaseLength value '{parts[3]}' for texture '{texture.Name}'.");
            }
            else
                BaseLength = defaultBaseLength;
        }

        public override readonly string ToString()
        {
            string cornerInfo = IsCorner ? $", CornerType: {CornerType}" : string.Empty;
            return $"{{Texture: {Texture.Name}, SupportedSides: {SupportedSides}, Base: ({BaseX}, {BaseY}), BaseLength: {BaseLength}, IsCorner: {IsCorner}{cornerInfo}}}";
        }
    }

    /// <summary>
    /// Represents a line of foliage drawn between two points.
    /// The points must have the same Y value, and the first point must be more left than the second.
    /// </summary>
    public sealed class Foliage : IDrawable, IDisposable
    {
        private readonly record struct FoliageTextureDrawInfo(Texture2D Texture, Vector2 Pos, bool DrawMirrored);

        private readonly FoliageEngine _engine;

        private readonly int _seed;

        private Vector2 _point1;
        /// <summary>
        /// Gets or sets the first point. Must have the same Y value as <see cref="Point2"/> and a lower X value.
        /// Recalculates foliage positions when set.
        /// Use <see cref="SetPoints(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2)"/> for setting both points to prevent recalculating foliage texture positions twice.
        /// </summary>
        public Vector2 Point1
        {
            get => _point1;
            set
            {
                _point1 = value;
                UpdatePoints();
            }
        }

        private Vector2 _point2;
        /// <summary>
        /// Gets or sets the second point. Must have the same Y value as <see cref="Point1"/> and a higher X value.
        /// Recalculates foliage positions when set.
        /// Use <see cref="SetPoints(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2)"/> for setting both points to prevent recalculating foliage texture positions twice.
        /// </summary>
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
        /// Gets or sets the formula that determines foliage reach dynamically based on position along the <see cref="Line"/>.
        /// The input parameter (0 to 1) represents how far the current point is across the line.
        /// The output value will be clamped to <see cref="MaxReach"/>.
        /// <code>Math.Min(ReachFormula.Invoke(lerpAmount), MaxReach)</code>
        /// </summary>
        public Func<float, int> ReachFormula { get; set; }

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

        public bool _drawCorners;
        /// <summary>
        /// Gets or sets if this <see cref="Foliage"/> instance will draw corner specific foliage textures.
        /// </summary>
        public bool DrawCorners
        {
            get => _drawCorners;
            set
            {
                _drawCorners = value;
                UpdatePoints();
            }
        }

        private int _count;
        private bool _isDisposed;
        private List<FoliageTextureDrawInfo> _cache;
        private List<Vector2> _pointCache;

        /// <param name="p1">The first point. Must have the same Y value as <paramref name="p2"/> and a lower X value.</param>
        /// <param name="p2">The second point. Must have the same Y value as <paramref name="p1"/> and a higher X value.</param>
        public Foliage(FoliageEngine engine, Vector2 p1, Vector2 p2)
        {
            _engine = engine;

            _point1 = p1;
            _point2 = p2;

            _count = (int)Vector2.Distance(p1, p2) / 40;
            //_count = 4;

            _cache = new List<FoliageTextureDrawInfo>(_count);
            _pointCache = new List<Vector2>(_count);
            _seed = engine.Register(this);

            MaxReach = int.MaxValue;
            ReachFormula = m => int.MaxValue;

            UpdatePoints(); // _count gets set here
        }

        private void UpdatePoints()
        {
            static float Hash01(int x) // [0, 1]
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
            if (_point1.X > _point2.X) throw new InvalidOperationException("Point1 must be more left than Point2.");
            if (_point1 == _point2) throw new InvalidOperationException("Points cannot be the same.");

            //Console.WriteLine("update points for " + _seed);

            _cache.Clear();
            _pointCache.Clear();

            float length = Vector2.Distance(_point1, _point2);

            float minSpacingMod = (1f / _count) * 0.5f;

            float segmentLengthMod = 1f / _count; // basically length of each segment each point can occupy if it would be nicelly balanced. (1/2)

            HashSet<Texture2D> addedTextures = new HashSet<Texture2D>();
            float lastPlacedFarBoundEndAlongLine = -1; // 1d position of last placed along the line + half texture width. (NOT A MODIFIER)
            float overlapMod = .7f; // more = less overlap allowed. (max 1)

            if (DrawCorners && Hash01(unchecked(_seed * 15 * (int)length)) > 0.66f) // corner 1
            {
                FoliageTexture? foliageTexture = _engine.GetFoliageTexture(_seed, 1, (int)length, Math.Min(ReachFormula.Invoke(0f), MaxReach), CornerType.Top, false, out Vector2 offset);

                if (foliageTexture is not null)
                {
                    _cache.Add(new FoliageTextureDrawInfo(foliageTexture.Value.Texture, _point1 + offset, false));
                    lastPlacedFarBoundEndAlongLine += foliageTexture.Value.BaseLength;
                }
            }

            for (int i = 0; i < _count; i++)
            {
                float jitter = Hash01(_seed * (i + 1)) * (segmentLengthMod - minSpacingMod); // its not. (2/2)
                float lerpAmount = i * segmentLengthMod + jitter;

                Vector2 basePos = Vector2.Lerp(_point1, _point2, lerpAmount);
                _pointCache.Add(basePos);

                float distToLeft = Math.Abs(basePos.X - _point1.X);
                float distToRight = Math.Abs(basePos.X - _point2.X);
                int spaceToEnds = (int)Math.Min(distToLeft, distToRight);

                // position has already been desided, but this makes sure larger textures dont overlap with the ones that came before.
                int spaceToPrevious = lastPlacedFarBoundEndAlongLine == -1 ? STOLON.VWidth : (int)((((basePos - _point1).X - lastPlacedFarBoundEndAlongLine)) / overlapMod);

                int maxSpace = Math.Min(spaceToEnds * 2, spaceToPrevious * 2); // * 2 because textures are centered on the basePos. (x only)
                bool drawMirrored = unchecked((_seed + i) & 1) == 0;

                FoliageTexture? foliageTexture = _engine.GetFoliageTexture(_seed, i, maxSpace, Math.Min(ReachFormula.Invoke(lerpAmount), MaxReach), null, drawMirrored, out Vector2 offset);

                if (foliageTexture is null)
                    continue;

                Vector2 drawPos = basePos + offset; // because basePos is centered (x only), offset to implement all that texture metadata and proper offset to draw texture centered.

                NumberHelper.OnPixel(ref drawPos);

                if (!addedTextures.Contains(foliageTexture.Value.Texture)) // no duplicate textures.
                {
                    _cache.Add(new FoliageTextureDrawInfo(foliageTexture.Value.Texture, drawPos, drawMirrored));
                    addedTextures.Add(foliageTexture.Value.Texture);
                    lastPlacedFarBoundEndAlongLine = (basePos - _point1).X + foliageTexture.Value.BaseLength * 0.5f;
                }
            }

            if (DrawCorners && Hash01(unchecked(_seed * 33 * (int)length)) > 0.33f) // higher chance than first corner because of the higher change GetTexture fails
            {
                FoliageTexture? foliageTexture = _engine.GetFoliageTexture(_seed, 1, (int)(length - lastPlacedFarBoundEndAlongLine), Math.Min(ReachFormula.Invoke(1f), MaxReach), CornerType.Top, true, out Vector2 offset);

                if (foliageTexture is not null)
                    _cache.Add(new FoliageTextureDrawInfo(foliageTexture.Value.Texture, _point2 + offset, true));
            }
        }

        /// <summary>
        /// Sets both <see cref="Point1"/> and <see cref="Point2"/> at once. Use this to prevent recalculating foliage twice when setting both points.
        /// </summary>
        public void SetPoints(Vector2 p1, Vector2 p2)
        {
            _point1 = p1;
            _point2 = p2;

            UpdatePoints();
        }

        public void Draw(DrawingContext drawingContext)
        {
            //drawingContext.DrawPoint(_point1, Color.Blue, 6);
            //drawingContext.DrawPoint(_point2, Color.Blue, 6);

            //for (int i = 0; i < _pointCache.Count; i++)
            //{
            //    drawingContext.DrawPoint(_pointCache[i], Color.Red, 6);
            //}

            for (int i = 0; i < _cache.Count; i++)
            {
                drawingContext.Draw(_cache[i].Texture, _cache[i].Pos, effects: _cache[i].DrawMirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
        }

        /// <summary>
        /// Gets a <see cref="Foliage"/> instance spanning the top of the rectangle.
        /// </summary>
        /// <returns>A <see cref="Foliage"/> instance spanning the top of the rectangle.</returns>
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

    internal enum CornerType
    {
        Top,
        Bottom,
    }

    [Dependency(ServiceLifetime.Singleton)]
    public sealed class FoliageEngine : IUpdatable
    {
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

            _foliageTextures = foliageTextures.Select(t => new FoliageTexture(t)).OrderBy(t => t.Texture.Name).ToArray();
        }

        internal int Register(Foliage foliage)
        {
            _foliages.Add(new WeakReference<Foliage>(foliage));

            int seed = _random.Next();

            unchecked
            {
                seed *= foliage.GetHashCode();
                seed = Math.Abs(seed);
            }

            return seed;
        }

        internal void Deregister(Foliage foliage)
        {
            _foliages.RemoveAll(wr => wr.TryGetTarget(out Foliage? f) && f == foliage);
        }

        internal FoliageTexture? GetFoliageTexture(int seed, int index, int sizeX, int sizeY, CornerType? cornerType, bool mirrored, out Vector2 offset)
        {
            int foliageTextureIndex;
            FoliageTexture[] availableFoliageTextures = _foliageTextures
                .Where(f => f.CornerType == cornerType &&
                            f.BaseLength < sizeX &&
                            f.Texture.Bounds.Height - f.BaseY < sizeY)
                .ToArray();

            if (availableFoliageTextures.Length == 0)
            {
                //Console.WriteLine("failed for (" + sizeX + ", " + sizeY + ")");
                offset = default;
                return null;
            }

            unchecked
            {
                int hash = seed;
                hash = hash * 397 ^ index;
                hash = hash * 397 ^ (sizeX << 16) | (sizeY & 0xFFFF); // might wanna remove this later. (causes alot of texture switches when changing point to point distances)
                hash = hash * 397 ^ (mirrored ? 1 : 0);
                hash = Math.Abs(hash);
                foliageTextureIndex = hash % availableFoliageTextures.Length;
            }

            FoliageTexture result = availableFoliageTextures[foliageTextureIndex];
            float baseYOffset = -result.Texture.Height + result.BaseY;

            if (result.IsCorner)
            {
                offset = mirrored
                    ? new Vector2(result.BaseX - result.Texture.Width, baseYOffset)
                    : new Vector2(-result.BaseX, baseYOffset);
            }
            else
            {
                float baseXOffset = mirrored
                    ? -(result.Texture.Width - (result.BaseLength + result.BaseX)) - (result.BaseLength / 2) // its never easy...
                    : -result.BaseX - (result.BaseLength / 2);

                offset = new Vector2(baseXOffset, baseYOffset);
            }

            //Console.WriteLine(result.Texture.Name + " for (" + sizeX + ", " + sizeY + ")" + (mirrored ? " [mirrored]" : string.Empty));

            return result;
        }

        public void Update(int elapsedMilliseconds) // doesnt do anything yet as i dont have a wind shader..... (also doesnt get called!!)
        {

        }
    }
}
