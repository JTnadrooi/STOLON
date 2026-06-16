using Autofac;
using MonoGame.Extended.BitmapFonts;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public sealed class Board : IComponent
    {
        public Camera2D Camera { get; }

        public BoardState InitialState { get; }

        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;
        private readonly IInputManager _input;
        private readonly ILogger _logger;
        private readonly Shell _shell;
        private readonly BoardState _state;

        private float _desiredZoom;
        private Vector2 _desiredCameraPos;

        public const int TileSize = 96;

        public Board(ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, ILogger logger, Shell shell, BoardState initialState)
        {
            _textures = textures;
            _fonts = fonts;
            _input = input;
            _logger = logger;
            _shell = shell;
            InitialState = initialState.DeepCopy();

            _state = initialState;
            _desiredZoom = MathF.Max(0.45f, 4f / initialState.Dimensions.X) * 0.6f;
            _desiredCameraPos = new Vector2(initialState.Dimensions.X / 2f, initialState.Dimensions.Y / 2f) * TileSize;

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

                if (_input.IsPressed(Keys.D))
                    _desiredCameraPos.X += 1;
                if (_input.IsPressed(Keys.A))
                    _desiredCameraPos.X += -1;

                if (_input.IsPressed(Keys.W))
                    _desiredCameraPos.Y += 1;
                if (_input.IsPressed(Keys.S))
                    _desiredCameraPos.Y += -1;
            }

            _desiredZoom = 0.3f * Math.Min((float)Camera.Dimensions.X / BoardWindow.InitialSize, (float)Camera.Dimensions.Y / BoardWindow.InitialSize);

            //if (_input.IsPressed(MouseButton.Right)) _desiredCameraPos += (_input.PreviousMouse.Position - _input.CurrentMouse.Position).ToVector2(); // do this smarterly.

            float smoothness = 0.003f;
            Camera.Zoom += (_desiredZoom - Camera.Zoom) * 0.1f + mouseStateCoefficient * smoothness;
            Camera.Position += (_desiredCameraPos - Camera.Position) * 0.1f + (worldMousePos - Camera.Position) * smoothness * Math.Abs(mouseStateCoefficient);

            Entity currentEntity = _state.CurrentEntity; // done to prevent Move.Apply changing it
            ReadOnlySpan<IMove> availableMoves = currentEntity.GetAvailableMoves(_state);
            if (currentEntity.MoveProvider.TryGetMove(_state, availableMoves, out IMove? move))
            {
                move.Apply(_state, currentEntity);
                if (currentEntity.HasWon(_state))
                {
                    _shell.WriteLine("Winner!");
                }
            }
        }

        public void Draw(DrawingContext drawingContext)
        {
            Matrix original = drawingContext.TransformMatrix!.Value;

            // compensate for camera zoom
            // and build a new transform: offset then camera
            Vector2 compensatedOffset = new Vector2(drawingContext.TransformMatrix.Value.M41, drawingContext.TransformMatrix.Value.M42) / Camera.Zoom;
            drawingContext.TransformMatrix = Matrix.CreateTranslation(compensatedOffset.X, compensatedOffset.Y, 0) * Camera.View;

            float totalWidth = _state.Dimensions.X * TileSize;
            float totalHeight = _state.Dimensions.Y * TileSize;

            for (int row = 0; row <= _state.Dimensions.Y; row++)
            {
                float y = row * TileSize;
                Vector2 start = new Vector2(0, y);
                Vector2 end = new Vector2(totalWidth, y);
                Camera.OnPixel(ref start);
                Camera.OnPixel(ref end);
                drawingContext.DrawLine(start, end, Color.White, thickness: Camera.AntiScale.X);
            }

            for (int col = 0; col <= _state.Dimensions.X; col++)
            {
                float x = col * TileSize;
                Vector2 start = new Vector2(x, 0);
                Vector2 end = new Vector2(x, totalHeight);
                Camera.OnPixel(ref start);
                Camera.OnPixel(ref end);
                drawingContext.DrawLine(start, end, Color.White, thickness: Camera.AntiScale.X);
            }

            for (int x = 0; x < _state.Dimensions.X; x++)
            {
                for (int y = 0; y < _state.Dimensions.Y; y++)
                {
                    Tile tile = _state.Tiles[x, y];
                    Vector2 tileWorldPos = tile.Position.ToVector2() * new Vector2(TileSize);
                    Camera.OnPixel(ref tileWorldPos);

                    int playerId = tile.GetOccupiedByPlayerIndex();
                    if (playerId != -1)
                    {
                        drawingContext.Draw(_textures.GetReference("player" + playerId + "_item-96"), tileWorldPos);
                    }

                    if (tile.HasAttribute<GravUpTileAttribute>())
                    {
                        drawingContext.Draw(_textures.GetReference("att-GravUp"), tileWorldPos + new Vector2(20, TileSize - 20), scale: Camera.AntiScale);
                    }
                }
            }

            drawingContext.TransformMatrix = original;

            //drawingContext.RegisterDraw(this, new Rectangle());
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

    public interface IMove
    {
        void Apply(BoardState state, Entity performer);
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

        public Rectangle GetHitbox()
        {
            return new Rectangle(Position * new Point(Board.TileSize), new Point(Board.TileSize));
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

                    if (y >= (int)(dimensions.Y / 2)) TileAttribute.ReplaceAttribute<GravDownTileAttribute, GravUpTileAttribute>(tileAttributes);

                    tiles[x, y] = new Tile(new Point(x, y), tileAttributes);
                }
            }

            return tiles;
        }
    }
}
