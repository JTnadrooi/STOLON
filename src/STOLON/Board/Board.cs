using Autofac;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public sealed class Board : IComponent
    {
        public Camera2D Camera { get; }

        public BoardState InitialState { get; }
        public BoardState State => _state;

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

            _desiredZoom = 0.3f * Math.Min((float)Camera.Dimensions.X / BoardWindow.InitialSizeX, (float)Camera.Dimensions.Y / BoardWindow.InitialSizeY);

            //if (_input.IsPressed(MouseButton.Right)) _desiredCameraPos += (_input.PreviousMouse.Position - _input.CurrentMouse.Position).ToVector2(); // do this smarterly.

            float smoothness = 0.003f;
            Camera.Zoom += (_desiredZoom - Camera.Zoom) * 0.1f + mouseStateCoefficient * smoothness;
            Camera.Position += (_desiredCameraPos - Camera.Position) * 0.1f + (worldMousePos - Camera.Position) * smoothness * Math.Abs(mouseStateCoefficient);

            Entity currentEntity = _state.CurrentEntity; // done to prevent Move.Apply changing it
            ReadOnlySpan<IMove> availableMoves = currentEntity.GetAvailableMoves(_state);
            if (currentEntity.MoveProvider.TryGetMove(_state, availableMoves, out IMove? move))
            {
                if (currentEntity.MoveProvider is UserMoveProvider)
                {
                    string? moveCommand = move.GetCommand();
                    if (moveCommand is not null)
                    {
                        _shell.SimulateUserCommand(moveCommand);
                    }
                    else move.Apply(_state, currentEntity);
                }
                else move.Apply(_state, currentEntity);
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
                    ref Tile tile = ref _state.Tiles[x, y];
                    Vector2 tileWorldPos = tile.Position.ToVector2() * new Vector2(TileSize);
                    Camera.OnPixel(ref tileWorldPos);

                    int playerId = tile.GetOccupiedByPlayerIndex();
                    if (playerId != -1)
                    {
                        drawingContext.Draw(_textures.GetReference("player" + playerId + "_item-96"), tileWorldPos);
                    }

                    if (!tile.IsOccupiedByPlayer())
                    {
                        if (tile.HasAttribute(TileAttributes.GravUp))
                        {
                            drawingContext.Draw(_textures.GetReference("att-GravUp"), tileWorldPos + new Vector2(10, TileSize - 10 - 8), scale: Camera.AntiScale);
                        }
                        if (tile.HasAttribute(TileAttributes.Disabled))
                        {
                            drawingContext.Draw(_textures.GetReference("att-Disabled"), tileWorldPos + new Vector2(10, TileSize - 10 - 8), scale: Camera.AntiScale);
                        }
                        drawingContext.DrawString(_fonts.Medium, Board.GetCoordsFromPoint(new Point(x, y)), tileWorldPos + new Vector2(10, 10), scale: Camera.AntiScale);
                    }

                    //drawingContext.Draw(_textures.GetReference("att-Disabled"), tileWorldPos + new Vector2(10, 10), scale: Camera.AntiScale);
                }
            }

            drawingContext.TransformMatrix = original;

            //drawingContext.RegisterDraw(this, new Rectangle());
        }

        public static string GetPlayerSymbol(int playerIndex) => playerIndex switch
        {
            0 => "[o]",
            1 => "[x]",
            2 => "[.]",
            3 => "[-]",
            4 => "[v]",
            5 => "[~]",
            _ => throw new Exception()
        };

        public static int GetColumnIndexFromChar(char column)
        {
            if (!char.IsLetter(column) || char.ToLowerInvariant(column) < 'a' || char.ToLowerInvariant(column) > 'z')
                throw new ArgumentOutOfRangeException(nameof(column), "Column must be a letter.");

            return char.ToLowerInvariant(column) - 'a' + 1;
        }

        public static char GetCharFromColumnIndex(int columnIndex)
        {
            if (columnIndex < 1 || columnIndex > 26)
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index must be between 1 and 26.");

            return (char)('a' + columnIndex - 1);
        }

        public static Point GetPointFromCoords(string coords) // ex: a1
        {
            if (string.IsNullOrWhiteSpace(coords) || coords.Length != 2)
                throw new ArgumentException("Invalid coordinates");

            char file = char.ToLower(coords[0]);
            char rank = coords[1];

            if (file < 'a' || rank < '1')
                throw new ArgumentException("Invalid coordinates");

            int x = file - 'a';
            int y = rank - '1';

            return new Point(x, y);
        }

        public static string GetCoordsFromPoint(Point point)
        {
            if (point.X < 0 || point.Y < 0)
                throw new ArgumentException("Invalid point");

            char file = (char)('a' + point.X);
            char rank = (char)('1' + point.Y);

            return $"{file}{rank}";
        }
    }

    public interface IMove : IEquatable<IMove>
    {
        void Apply(BoardState state, Entity performer);
        void Undo(BoardState state, Entity performer);

        string GetCommand();
    }

    public static class Move
    {

    }

    [Flags]
    public enum TileAttributes
    {
        None = 0,
        Player0Occupied = 1 << 0,
        Player1Occupied = 1 << 1,
        GravDown = 1 << 2,
        GravUp = 1 << 3,
        Solid = 1 << 4,

        Disabled0 = 1 << 5,
        Disabled1 = 1 << 6,

        Default = GravDown,
        Disabled = Disabled0 | Disabled1,
    }

    public readonly struct Tile : ICloneable
    {
        public Point Position { get; } // never changes, the rest is free to change though. It's cheaper to not allocate a new Tile every time a Tile changes.
        public TileAttributes Attributes { get; }

        public const int MaxAttributeCount = 32;

        public Tile(Point position, TileAttributes attributes)
        {
            Position = position;
            Attributes = attributes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute(TileAttributes attribute)
            => (Attributes & attribute) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasGravity() => HasAttribute(TileAttributes.GravDown) || HasAttribute(TileAttributes.GravUp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSolid() => HasAttribute(TileAttributes.Solid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupiedByPlayer() => GetOccupiedByPlayerIndex() != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOccupiedByPlayerIndex()
        {
            if (HasAttribute(TileAttributes.Player0Occupied)) return 0;
            if (HasAttribute(TileAttributes.Player1Occupied)) return 1;
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
        public Tile Clone() => new Tile(Position, Attributes);

        object ICloneable.Clone() => Clone();

        public override string ToString()
        {
            if (Attributes == TileAttributes.None)
                return $"{{Position: {Position}, Attributes: None}}";

            List<string> parts = new List<string>();

            foreach (TileAttributes value in Enum.GetValues(typeof(TileAttributes)))
            {
                if (value == TileAttributes.None)
                    continue;

                if ((Attributes & value) != 0)
                    parts.Add(value.ToString());
            }

            return $"{{Position: {Position}, Attributes: {string.Join(", ", parts)}}}";
        }

        public override int GetHashCode() => Position.GetHashCode();

        public static Tile[,] GetTiles(Point dimensions, bool random = false)
        {
            Tile[,] tiles = new Tile[dimensions.X, dimensions.Y];
            Random rnd = new Random();

            for (int x = 0; x < dimensions.X; x++)
            {
                for (int y = 0; y < dimensions.Y; y++)
                {
                    TileAttributes attributes = TileAttributes.Default;

                    if (y >= (int)(dimensions.Y / 2))
                    {
                        if (y >= dimensions.Y / 2)
                        {
                            attributes &= ~TileAttributes.GravDown;
                            attributes |= TileAttributes.GravUp;
                        }
                    }

                    tiles[x, y] = new Tile(new Point(x, y), attributes);
                }
            }

            return tiles;
        }
    }
}
