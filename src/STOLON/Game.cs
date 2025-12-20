using AsitLib.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace STOLON
{
    public partial class STOLON : Game
    {
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private DrawingContext _drawingContext;

        private GameEnvironment _environment;
        private int _desiredModifier;
        private Color[] _palette;
        private Texture2DCollection _textures;
        private Font2DCollection _fonts;
        private Point _oldWindowSize;

        public DiscordRichPresence DRP { get; private set; }
        //public Point VirtualDimensions => new Point(ASPECT_RATIO_X * VIRTUAL_MODIFIER, ASPECT_RATIO_Y * VIRTUAL_MODIFIER); //  (912, 513) (if vM = 57) - (480, 270) (if vM = 30)
        public Point DesiredDimensions => new Point(ASPECT_RATIO_X * _desiredModifier, ASPECT_RATIO_Y * _desiredModifier);
        public Point ScreenCenter => new Point(V_WIDTH / 2, V_HEIGHT / 2);
        public float ScreenScale { get; private set; }

        public GraphicsDeviceManager GraphicsDeviceManager => _graphics;
        public Color Color1 => _palette[0];
        public Color Color2 => _palette[1];


#pragma warning disable CS8618
        public STOLON()
#pragma warning restore CS8618
        {
            Instance = this;
            IsInitiated = true;

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = string.Empty; // heh
            IsMouseVisible = true;

            Debug = new Logger(header: "STOLON");
            Debug.Silent = false;
        }

        protected override void Initialize()
        {
            Debug.Log(">[s]initializing STOLON");
            DRP = new DiscordRichPresence();
            DRP.UpdateDetails("Initializing..");

            _oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);

            _desiredModifier = 57;

            _graphics.PreferredBackBufferWidth = DesiredDimensions.X;
            _graphics.PreferredBackBufferHeight = DesiredDimensions.Y;
            _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 0;
            _graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();


            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Debug.Success();
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
            Debug.Log(">[s]loading stolon content");

            _palette = [
                new Color(242, 251, 235), // #f2fbeb
                new Color(23, 18, 25), // #171219
            ];
            STOLON.Debug.Log("palette set.");

            STOLON.Config = new Configuration();
            STOLON.AudioEngine = new AudioEngine();

            STOLON.Textures = _textures = ResourceCollection.Load<Texture2DCollection>();
            STOLON.Fonts = _fonts = ResourceCollection.Load<Font2DCollection>();
            STOLON.Effects = ResourceCollection.Load<EffectResourceCollection>();
            STOLON.Audio = ResourceCollection.Load<CachedAudioResourceCollection>();
            STOLON.DrawingContext = _drawingContext = new DrawingContext();
            STOLON.Input = _input = new InputManager();
            STOLON.Tasks = new TaskHeap();
            STOLON.Environment = _environment = new GameEnvironment();
            _environment.Initialize();


            bool silenceConsole = !STOLON.Config.GetBool("debug.log.enable");
            if (silenceConsole) STOLON.Debug.Log("console will be silenced.");
            STOLON.Debug.Silent = silenceConsole;

            Debug.Success();
            //throw new Exception();

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            AudioEngine.Dispose();
            MediaPlayer.Stop();
            Textures.UnloadResources();
            Fonts.UnloadResources();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            int elapsedMilliseconds = gameTime.ElapsedGameTime.Milliseconds;

            if (IsActive)
            {
                ScreenScale = GraphicsDevice.Viewport.Bounds.Size.Y / (float)V_HEIGHT;
                _desiredModifier = (int)(VIRTUAL_MODIFIER * ScreenScale);

                STOLON.Input.Update(elapsedMilliseconds);
                STOLON.Tasks.Update(elapsedMilliseconds);
                STOLON.Environment.Update(elapsedMilliseconds);

                if (STOLON.Input.Focus == MouseFocus.None)
                {
                    if (STOLON.Input.IsClicked(Keys.F)) GoFullscreen();
                    if (STOLON.Input.IsClicked(Keys.S)) _drawingContext.Screenshot();
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _drawingContext.BeginScene();

            _environment.Draw(_drawingContext);
            _drawingContext.DrawString(STOLON.Fonts.Small, Version, new Vector2(V_WIDTH / 2 - STOLON.Fonts.Small.FastMeasure(Version).X / 2, 500));
            _drawingContext.DrawRectangle(STOLON.Instance.GetVirtualBounds(), Color.White, 1);

            _drawingContext.EndScene();

            base.Draw(gameTime);
        }
    }

    public partial class STOLON
    {
#nullable disable
        public static bool IsInitiated { get; private set; }

        #region INSTANCE

        private static T ThrowIfNotInitiated<T>(T value) => IsInitiated ? value : throw new InvalidOperationException("Instance is not initiated.");

        private static class BackingFields
        {
            public static Texture2DCollection _textures;
            public static Font2DCollection _fonts;
            public static EffectResourceCollection _effects;
            public static CachedAudioResourceCollection _audio;
            public static AudioEngine _audioEngine;
            public static Logger _debug;
            public static GameEnvironment _environment;
            public static InputManager _input;
            public static SceneManager _sceneManager;
            public static Interface _ui;
            public static Configuration _config;
            public static DrawingContext _drawingContext;
            public static TaskHeap _tasks;
            public static STOLON _instance;
        }

        public static STOLON Instance { get => ThrowIfNotInitiated(BackingFields._instance); private set => BackingFields._instance = value; }
        public static Texture2DCollection Textures { get => ThrowIfNotInitiated(BackingFields._textures); set => BackingFields._textures = value; }
        public static Font2DCollection Fonts { get => ThrowIfNotInitiated(BackingFields._fonts); private set => BackingFields._fonts = value; }
        public static EffectResourceCollection Effects { get => ThrowIfNotInitiated(BackingFields._effects); private set => BackingFields._effects = value; }
        public static CachedAudioResourceCollection Audio { get => ThrowIfNotInitiated(BackingFields._audio); private set => BackingFields._audio = value; }
        public static AudioEngine AudioEngine { get => ThrowIfNotInitiated(BackingFields._audioEngine); private set => BackingFields._audioEngine = value; }
        public static Logger Debug { get => BackingFields._debug; set => BackingFields._debug = value; }
        public static GameEnvironment Environment { get => ThrowIfNotInitiated(BackingFields._environment); private set => BackingFields._environment = value; }
        public static InputManager Input { get => ThrowIfNotInitiated(BackingFields._input); private set => BackingFields._input = value; }
        public static SceneManager SceneManager { get => BackingFields._sceneManager; internal set => BackingFields._sceneManager = value; }
        public static Interface UI { get => BackingFields._ui; internal set => BackingFields._ui = value; }
        public static Configuration Config { get => BackingFields._config; internal set => BackingFields._config = value; }
        public static DrawingContext DrawingContext { get => BackingFields._drawingContext; internal set => BackingFields._drawingContext = value; }
        public static TaskHeap Tasks { get => BackingFields._tasks; internal set => BackingFields._tasks = value; }

        #endregion
#nullable enable

        public static string Version { get; }

        public const int V_WIDTH = ASPECT_RATIO_X * VIRTUAL_MODIFIER;
        public const int V_HEIGHT = ASPECT_RATIO_Y * VIRTUAL_MODIFIER;
        public const int ASPECT_RATIO_X = 16;
        public const int ASPECT_RATIO_Y = 9;
        public const int VIRTUAL_MODIFIER = 57;
        public const float ASPECT_RATIO_FLOAT = ASPECT_RATIO_X / (float)ASPECT_RATIO_Y;

        static STOLON()
        {
            Version = File.ReadAllText(".version");
        }

        public static T[] Scan<T>() where T : class
        {
            Debug.Log($"called assembly scan for type '{typeof(T).FullName}\".");
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Select(t => (Activator.CreateInstance(t) as T)!).ToArray();
        }
    }
}
