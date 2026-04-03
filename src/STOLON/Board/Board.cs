using Autofac;
using MonoGame.Extended.BitmapFonts;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public partial class Board : IComponent
    {
        public Camera2D Camera { get; }

        public float MaxDeltaZoom => SmoothnessModifier * 10f;
        public float ZoomIntensity => (Camera.Zoom - _desiredZoom) / MaxDeltaZoom;
        public float SmoothnessModifier => 0.003f;
        public int TurnCount { get; private set; }
        public BoardState InitialState { get; }

        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;
        private readonly IInputManager _input;
        private readonly ILogger _logger;
        private readonly BoardState _state;
        private readonly float _desiredZoom;

        private Vector2 _desiredCameraPos;
        public const int TileSize = 96;

        public Board(ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, ILogger logger, BoardState initialBoardState)
        {
            _textures = textures;
            _fonts = fonts;
            _input = input;
            _logger = logger;

            InitialState = initialBoardState.DeepCopy();
            TurnCount = 0;

            _state = initialBoardState;
            _desiredZoom = MathF.Max(0.45f, 4f / initialBoardState.Dimensions.X);
            _desiredCameraPos = Vector2.Zero;

            Camera = new Camera2D()
            {
                Position = _desiredCameraPos,
                Zoom = 1f
            };
        }

        public void Update(int elapsedMilliseconds)
        {
            Vector2 worldMousePos = Camera.Unproject(_input.Mouse.Position);

            int mouseStateCoefficient = _input.Mouse.GetCoefficient();

            if (_input.IsPressed(Keys.LeftShift))
            {
                if (mouseStateCoefficient == 0) mouseStateCoefficient = 1;
                if (_input.IsPressed(Keys.A))
                    _desiredCameraPos.X -= 1;
                if (_input.IsPressed(Keys.D))
                    _desiredCameraPos.X += 1;
                if (_input.IsPressed(Keys.W))
                    _desiredCameraPos.Y -= 1;
                if (_input.IsPressed(Keys.S))
                    _desiredCameraPos.Y += 1;
            }

            //if (_input.IsPressed(MouseButton.Right)) _desiredCameraPos += (_input.PreviousMouse.Position - _input.CurrentMouse.Position).ToVector2(); // do this smarterly.

            Camera.Zoom += (_desiredZoom - Camera.Zoom) * 0.1f + mouseStateCoefficient * SmoothnessModifier;
            Camera.Position += (_desiredCameraPos - Camera.Position) * 0.1f + (worldMousePos - Camera.Position) * SmoothnessModifier * Math.Abs(mouseStateCoefficient);
        }

        public void Draw(DrawingContext drawingContext)
        {
            Matrix original = drawingContext.TransformMatrix!.Value;

            // extract the original offset from the current TransformMatrix.
            Vector2 offset = new Vector2(drawingContext.TransformMatrix.Value.M41, drawingContext.TransformMatrix.Value.M42);

            // compensate for camera zoom.
            Vector2 compensatedOffset = offset / Camera.Zoom;

            // build a new transform: offset then camera.
            drawingContext.TransformMatrix = Matrix.CreateTranslation(compensatedOffset.X, compensatedOffset.Y, 0) * Camera.View;

            for (int x = 0; x < _state.Dimensions.X; x++)
                for (int y = 0; y < _state.Dimensions.Y; y++)
                {
                    Tile tile = _state.Tiles[x, y];
                    Vector2 tileWorldPos = Camera.Project(tile.Position.ToVector2() * new Vector2(TileSize));
                    NumberHelper.OnPixel(ref tileWorldPos);

                    drawingContext.Draw(tile.GetTexture(_textures), tileWorldPos);
                    int playerid = tile.GetOccupiedByPlayerIndex();
                    if (playerid != -1)
                    {
                        drawingContext.Draw(_textures.GetReference("player" + playerid + "_item-96"), tileWorldPos);
                    }
                    else if (tile.HasAttribute<GravDownTileAttribute>()) drawingContext.DrawString(_fonts.Medium, string.Empty, tileWorldPos + new Vector2(10));
                    else if (tile.HasAttribute<GravUpTileAttribute>()) drawingContext.DrawString(_fonts.Medium, "^", tileWorldPos + new Vector2(10));
                    else drawingContext.DrawString(_fonts.Medium, "Z", tileWorldPos + new Vector2(10));
                }

            drawingContext.TransformMatrix = original;
        }

        public string GetPlayerSymbol(int playerIndex) => playerIndex switch
        {
            0 => "[o]",
            1 => "[x]",
            2 => "[.]",
            3 => "[-]",
            4 => "[v]",
            5 => "[~]",
            _ => throw new Exception()
        };
    }

    public abstract class Move
    {
        public abstract void Act(BoardState boardState);
    }

    public sealed class Tile : ICloneable
    {
        public Point Position { get; } // never changes, the rest is free to change though. It's cheaper to not allocate a new Tile every time a Tile changes.
        public HashSet<TileAttribute> Attributes { get; set; }

        public Tile(Point position, HashSet<TileAttribute>? attributes = null)
        {
            Position = position;
            Attributes = attributes ?? new HashSet<TileAttribute>();
        }

        public bool HasAnyAttribute(params ReadOnlySpan<TileAttribute> attributes)
        {
            if (attributes.Length == 0) throw new Exception();
            for (int i = 0; i < attributes.Length; i++)
                if (HasAttribute(attributes[i])) return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute<TTileAttribute>() where TTileAttribute : TileAttribute
            => HasAttribute(TileAttribute.Get<TTileAttribute>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute(TileAttribute attribute)
            => Attributes.Contains(attribute);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasGravity() => HasAttribute<GravDownTileAttribute>() || HasAttribute<GravUpTileAttribute>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSolid() => HasAttribute<SolidTileAttribute>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupiedByPlayer() => GetOccupiedByPlayerIndex() != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOccupiedByPlayerIndex()
        {
            if (HasAttribute<Player0OccupiedTileAttribute>()) return 0;
            if (HasAttribute<Player1OccupiedTileAttribute>()) return 1;
            return -1;
        }

        public Texture2D GetTexture(ITexture2DCollection textures)
        {
            return textures.GetReference("box-96");
        }

        // for multithread magic.
        public Tile Clone() => new Tile(Position, new HashSet<TileAttribute>(Attributes));

        object ICloneable.Clone() => Clone();

        public override int GetHashCode() => Position.GetHashCode();

        public static Tile[,] GetTiles(Point dimensions, bool random = false)
        {
            Tile[,] tiles = new Tile[dimensions.X, dimensions.Y];
            Random rnd = new Random();

            for (int x = 0; x < dimensions.X; x++)
            {
                for (int y = 0; y < dimensions.Y; y++)
                {
                    HashSet<TileAttribute> tileAttributes = new HashSet<TileAttribute>(TileAttribute.DefaultAttributes);

                    if (y < (int)(dimensions.Y / 2)) TileAttribute.ReplaceAttribute<GravDownTileAttribute, GravUpTileAttribute>(tileAttributes);

                    tiles[x, y] = new Tile(new Point(x, y), tileAttributes);
                }
            }

            return tiles;
        }
    }
}
