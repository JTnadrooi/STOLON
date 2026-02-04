using Autofac;
using DiscordRPC.Logging;
using System.Collections.Generic;
using System.Reflection;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public sealed class STOLON : Game
    {
        private GraphicsDeviceManager _graphics;
        private int _desiredModifier;
        private Point _oldWindowSize;

        public DiscordRichPresence DRP { get; private set; }
        public Point DesiredDimensions => new Point(AspectRatioX * _desiredModifier, AspectRatioY * _desiredModifier);
        public Point ScreenCenter => new Point(VWidth / 2, VHeight / 2);
        public float ScreenScale { get; private set; }

        public GraphicsDeviceManager GraphicsDeviceManager => _graphics;

        private readonly IRichLogger _logger;
        private readonly IInputManager _input;
        private readonly ITaskHeap _tasks;
        private readonly IConfiguration _config;
        private Environment _environment;

#pragma warning disable CS8618
        public STOLON(IRichLogger logger,
            IInputManager input,
            ITaskHeap tasks,
            IConfiguration config)
#pragma warning restore CS8618
        {
            _logger = logger;
            _input = input;
            _tasks = tasks;
            _config = config;

            _instance = this;

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = string.Empty; // heh
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            DRP = new DiscordRichPresence();

            _oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);

            _desiredModifier = 57;

            _graphics.PreferredBackBufferWidth = DesiredDimensions.X;
            _graphics.PreferredBackBufferHeight = DesiredDimensions.Y;
            _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 0;
            _graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();


            Window.ClientSizeChanged += Window_ClientSizeChanged;
            base.Initialize();
        }

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            Window.ClientSizeChanged -= Window_ClientSizeChanged;

            int newWidth = Window.ClientBounds.Width;
            int newHeight = Window.ClientBounds.Height;

            if (newWidth != _oldWindowSize.X)
            {
                _graphics.PreferredBackBufferWidth = newWidth;
                _graphics.PreferredBackBufferHeight = (int)(newWidth / AspectRatioFloat);
            }
            else if (newHeight != _oldWindowSize.Y)
            {
                _graphics.PreferredBackBufferWidth = (int)(newHeight * AspectRatioFloat);
                _graphics.PreferredBackBufferHeight = newHeight;
            }

            _graphics.ApplyChanges();

            _oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
            ScreenScale = (GraphicsDevice.Viewport.Bounds.Size.Y / (float)VHeight);
            _desiredModifier = (int)(VirtualModifier * ScreenScale);

            _drawingContext.UpdateResolution();
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }
        public void GoFullscreen()
        {
            if (_graphics.IsFullScreen) SetBackBufferSize(new Point(VWidth, VHeight));
            else
            {
                SetBackBufferSize(new Point(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height));
                _graphics.ApplyChanges();
            }
            _graphics.ToggleFullScreen();
            _graphics.ApplyChanges();
        }

        public Rectangle GetVirtualBounds() => new Rectangle(Point.Zero, GetVirtualDimensions());
        public Point GetVirtualDimensions() => new Point(VWidth, VHeight);

        private void SetBackBufferSize(Point size)
        {
            _graphics.PreferredBackBufferWidth = size.X;
            _graphics.PreferredBackBufferHeight = size.Y;
        }

        protected override void LoadContent()
        {
            _logger.Log(">[s]loading stolon content");


            int loadCount = 0;
            foreach (IResourceCollection resourceCollection in Services.Resolve<IEnumerable<IResourceCollection>>())
            {
                if (resourceCollection.IsLoaded) continue;

                resourceCollection.LoadResources();
                loadCount++;
            }
            if (loadCount != 4) throw new Exception(loadCount.ToString());  // 4 because of the differnt asset types, ignore this. This is just checking if nothing is loaded more than once.

            _drawingContext = Services.Resolve<DrawingContext>();

            _environment = Services.Resolve<Environment>();
            _environment.Initialize();

            bool silenceConsole = !_config.GetBool("debug.log.enable");
            if (silenceConsole) _logger.Log("console will be silenced.");
            _logger.Silent = silenceConsole;

            base.LoadContent();
            _logger.Success();
        }

        protected override void UnloadContent()
        {
            MediaPlayer.Stop();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            int elapsedMilliseconds = gameTime.ElapsedGameTime.Milliseconds;

            if (IsActive)
            {
                ScreenScale = GraphicsDevice.Viewport.Bounds.Size.Y / (float)VHeight;
                _desiredModifier = (int)(VirtualModifier * ScreenScale);

                _input.Update(elapsedMilliseconds);
                _tasks.Update(elapsedMilliseconds);
                _environment.Update(elapsedMilliseconds);

                _input.PostUpdate(elapsedMilliseconds);

                if (_input.IsPressed(Keys.LeftControl))
                {
                    if (_input.IsClicked(Keys.F)) GoFullscreen();
                    if (_input.IsClicked(Keys.S)) _drawingContext.Screenshot();
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (IsActive)
            {
                _drawingContext.BeginScene();

                _environment.Draw(_drawingContext);
                //_drawingContext.DrawString(_fonts.Small, Version, new Vector2(V_WIDTH / 2 - _fonts.Small.FastMeasure(Version).X / 2, 500));
                _drawingContext.DrawRectangle(STOLON.Instance.GetVirtualBounds(), Color.White, 1);

                _drawingContext.EndScene();
            }

            base.Draw(gameTime);
        }

        public static readonly Rectangle Bounds;
        public static Color Color1 => _palette[0];
        public static Color Color2 => _palette[1];
        public static string Version { get; } = File.ReadAllText(".version");
        public static bool IsInitiated => _instance is not null;
        public static STOLON Instance => _instance ?? throw new InvalidOperationException("STOLON is not initiated.");
        public static DrawingContext DrawingContext => _drawingContext ?? throw new InvalidOperationException("STOLON is not initiated.");

        private static STOLON? _instance;
        private static IContainer? _services;
        private static DrawingContext? _drawingContext;
        private readonly static Color[] _palette;

        public new static IContainer Services
        {
            get => _services ?? throw new InvalidOperationException("Services is not set.");
            set
            {
                if (_services is not null) throw new InvalidOperationException("Container can only be set once.");
                _services = value;
            }
        }

        static STOLON()
        {
            _palette = [
                new Color(242, 251, 235), // #f2fbeb
                new Color(23, 18, 25), // #171219
            ];

            Bounds = new Rectangle(0, 0, VWidth, VHeight);
        }

        public const int VWidth = AspectRatioX * VirtualModifier;
        public const int VHeight = AspectRatioY * VirtualModifier;
        public const int AspectRatioX = 16;
        public const int AspectRatioY = 9;
        public const int VirtualModifier = 57;
        public const float AspectRatioFloat = AspectRatioX / (float)AspectRatioY;
    }
}
