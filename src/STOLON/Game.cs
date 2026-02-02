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
        private DrawingContext _drawingContext;
        private int _desiredModifier;
        private Color[] _palette;
        private Point _oldWindowSize;

        public DiscordRichPresence DRP { get; private set; }
        public Point DesiredDimensions => new Point(ASPECT_RATIO_X * _desiredModifier, ASPECT_RATIO_Y * _desiredModifier);
        public Point ScreenCenter => new Point(V_WIDTH / 2, V_HEIGHT / 2);
        public float ScreenScale { get; private set; }

        public GraphicsDeviceManager GraphicsDeviceManager => _graphics;
        public Color Color1 => _palette[0];
        public Color Color2 => _palette[1];

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

            Instance = this;
            IsInitiated = true;

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
                _graphics.PreferredBackBufferHeight = (int)(newWidth / ASPECT_RATIO_FLOAT);
            }
            else if (newHeight != _oldWindowSize.Y)
            {
                _graphics.PreferredBackBufferWidth = (int)(newHeight * ASPECT_RATIO_FLOAT);
                _graphics.PreferredBackBufferHeight = newHeight;
            }

            _graphics.ApplyChanges();

            _oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
            ScreenScale = (GraphicsDevice.Viewport.Bounds.Size.Y / (float)V_HEIGHT);
            _desiredModifier = (int)(VIRTUAL_MODIFIER * ScreenScale);

            _drawingContext.UpdateResolution();
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }
        public void GoFullscreen()
        {
            if (_graphics.IsFullScreen) SetBackBufferSize(new Point(V_WIDTH, V_HEIGHT));
            else
            {
                SetBackBufferSize(new Point(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height));
                _graphics.ApplyChanges();
            }
            _graphics.ToggleFullScreen();
            _graphics.ApplyChanges();
        }

        public Rectangle GetVirtualBounds() => new Rectangle(Point.Zero, GetVirtualDimensions());
        public Point GetVirtualDimensions() => new Point(V_WIDTH, V_HEIGHT);

        private void SetBackBufferSize(Point size)
        {
            _graphics.PreferredBackBufferWidth = size.X;
            _graphics.PreferredBackBufferHeight = size.Y;
        }

        protected override void LoadContent()
        {
            _logger.Log(">[s]loading stolon content");

            _palette = [
                new Color(242, 251, 235), // #f2fbeb
                new Color(23, 18, 25), // #171219
            ];

            int loadCount = 0;
            foreach (IResourceCollection resourceCollection in Services.Resolve<IEnumerable<IResourceCollection>>())
            {
                if (resourceCollection.IsLoaded) continue;

                resourceCollection.LoadResources();
                loadCount++;
            }
            if (loadCount != 4) throw new Exception(loadCount.ToString());  // 4 because of the differnt asset types, ignore this. This is just checking if nothing is loaded more than once.

            Console.WriteLine(Services.Resolve<IEnumerable<IResourceCollection>>().All(c => c.IsLoaded)); // true, rest below is false. (even though the texture collection implements all these, and of course IResourceCollection) 
            Console.WriteLine(Services.Resolve<ITexture2DCollection>().IsLoaded);
            Console.WriteLine(Services.Resolve<Texture2DCollection>().IsLoaded);
            Console.WriteLine(Services.Resolve<IResourceCollection<Texture2D>>().IsLoaded);

            STOLON.DrawingContext = _drawingContext = Services.Resolve<DrawingContext>();

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
                ScreenScale = GraphicsDevice.Viewport.Bounds.Size.Y / (float)V_HEIGHT;
                _desiredModifier = (int)(VIRTUAL_MODIFIER * ScreenScale);

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
            _drawingContext.BeginScene();

            _environment.Draw(_drawingContext);
            //_drawingContext.DrawString(_fonts.Small, Version, new Vector2(V_WIDTH / 2 - _fonts.Small.FastMeasure(Version).X / 2, 500));
            _drawingContext.DrawRectangle(STOLON.Instance.GetVirtualBounds(), Color.White, 1);

            _drawingContext.EndScene();

            base.Draw(gameTime);
        }

#nullable disable
        public static bool IsInitiated { get; private set; }

        public static STOLON Instance { get; private set; }

        public static DrawingContext DrawingContext { get; private set; }

        private static IContainer _container;
        public new static IContainer Services
        {
            get => _container;
            set
            {
                if (_container is not null) throw new InvalidOperationException("Container can only be set once.");
                _container = value;
            }
        }

#nullable enable

        public static string Version { get; } = File.ReadAllText(".version");

        public const int V_WIDTH = ASPECT_RATIO_X * VIRTUAL_MODIFIER;
        public const int V_HEIGHT = ASPECT_RATIO_Y * VIRTUAL_MODIFIER;
        public const int ASPECT_RATIO_X = 16;
        public const int ASPECT_RATIO_Y = 9;
        public const int VIRTUAL_MODIFIER = 57;
        public const float ASPECT_RATIO_FLOAT = ASPECT_RATIO_X / (float)ASPECT_RATIO_Y;
    }
}
