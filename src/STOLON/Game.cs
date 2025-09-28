using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Debug;
using MonoGame.Extended;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using DiscordRPC;
using DiscordRPC.Events;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;



namespace STOLON
{
    public partial class STOLON : Game
    {
        private GraphicsDeviceManager _graphics;
        private GameInput _input;
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
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Debug = new DebugStream(header: "STOLON");
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

            STOLON.Config = new GameConfig();
            STOLON.Audio = new AudioEngine();
            STOLON.Textures = _textures = new Texture2DCollection(Content);
            STOLON.Fonts = _fonts = new Font2DCollection(Content);
            STOLON.DrawingContext = _drawingContext = new DrawingContext();
            STOLON.Input = _input = new GameInput();
            STOLON.Tasks = new TaskHeap();
            STOLON.Environment = _environment = new GameEnvironment();
            STOLON.VersionString = File.ReadAllText(".version");
            _environment.Initialize();

            if (!STOLON.Config.GetBool("Graphics.crt_enable")) _drawingContext.DisableShader("crt");

            Debug.Success();

            bool silenceConsole = !STOLON.Config.GetBool("Debug.log_enable");
            if (silenceConsole) STOLON.Debug.Log("console will be silenced.");
            STOLON.Debug.Silent = silenceConsole;

            base.LoadContent();
        }
        protected override void UnloadContent()
        {
            Audio.Dispose();
            MediaPlayer.Stop();
            Textures.UnloadContent();
            Fonts.UnloadContent();
            base.UnloadContent();
        }
        protected override void Update(GameTime gameTime)
        {
            //[DllImport("kernel32.dll")]
            //static extern IntPtr GetConsoleWindow();
            //[DllImport("user32.dll")]
            //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
            //const int SW_HIDE = 0;
            ////const int SW_SHOW = 5;
            //if (!Config.GetBool("Debug.console_enable") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ShowWindow(GetConsoleWindow(), SW_HIDE);
            if (IsActive)
            {
                STOLON.Input.PreviousMouse = STOLON.Input.CurrentMouse;
                STOLON.Input.CurrentMouse = Mouse.GetState();

                STOLON.Input.PreviousKeyboard = STOLON.Input.CurrentKeyboard;
                STOLON.Input.CurrentKeyboard = Keyboard.GetState();

                if (!GraphicsDevice.Viewport.Bounds.Contains(STOLON.Input.CurrentMouse.Position)) STOLON.Input.Domain = GameInput.MouseDomain.OfScreen;
                else if (STOLON.UI.Textframe.DialogueBounds.Contains(STOLON.Input.VirtualMousePos)) STOLON.Input.Domain = GameInput.MouseDomain.Dialogue;
                else if (STOLON.StateManager.IsCurrent<BoardGameState>() && STOLON.Input.VirtualMousePos.X > (int)STOLON.StateManager.GetCurrent<BoardGameState>().Line1X && STOLON.Input.VirtualMousePos.X < (int)STOLON.StateManager.GetCurrent<BoardGameState>().Line2X) STOLON.Input.Domain = GameInput.MouseDomain.Board;
                else STOLON.Input.Domain = GameInput.MouseDomain.UserInterfaceLow;

                ScreenScale = (GraphicsDevice.Viewport.Bounds.Size.Y / (float)V_HEIGHT);
                //ScreenScale = (GraphicsDevice.Viewport.Bounds.Size.ToVector2() / new Vector2(V_WIDTH, V_HEIGHT)).Y;
                _desiredModifier = (int)(VIRTUAL_MODIFIER * ScreenScale);

                STOLON.Tasks.Update(gameTime.ElapsedGameTime.Milliseconds);
                _environment.Update(gameTime.ElapsedGameTime.Milliseconds);

                if (STOLON.Input.IsClicked(Keys.F)) GoFullscreen();
                if (STOLON.Input.IsClicked(Keys.S)) _drawingContext.Screenshot();
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            int elapsedMilliseconds = gameTime.ElapsedGameTime.Milliseconds;
            _drawingContext.BeginScene();

            _environment.Draw(_drawingContext);
            _drawingContext.DrawString(STOLON.Fonts.Small, VersionString, new Vector2(V_WIDTH / 2 - STOLON.Fonts.Small.FastMeasure(VersionString).X / 2, 500));
            _drawingContext.DrawRectangle(STOLON.Instance.GetVirtualBounds(), Color.White, 1);

            _drawingContext.EndScene();

            base.Draw(gameTime);
        }
    }
    public partial class STOLON
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static STOLON Instance { get; private set; }
        public static Texture2DCollection Textures { get; private set; }
        public static Font2DCollection Fonts { get; private set; }
        public static AudioEngine Audio { get; private set; }
        public static DebugStream Debug { get; private set; }
        public static GameEnvironment Environment { get; private set; }
        public static GameInput Input { get; private set; }
        public static GameStateManager StateManager { get; internal set; }
        public static Interface UI { get; internal set; }
        public static GameConfig Config { get; internal set; }
        public static DrawingContext DrawingContext { get; internal set; }
        public static TaskHeap Tasks { get; internal set; }
        public static string VersionString { get; internal set; }
        public const int V_WIDTH = ASPECT_RATIO_X * VIRTUAL_MODIFIER;
        public const int V_HEIGHT = ASPECT_RATIO_Y * VIRTUAL_MODIFIER;
        public const int ASPECT_RATIO_X = 16;
        public const int ASPECT_RATIO_Y = 9;
        public const int VIRTUAL_MODIFIER = 57;
        public const float ASPECT_RATIO_FLOAT = ASPECT_RATIO_X / (float)ASPECT_RATIO_Y;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static T[] Scan<T>() where T : class
        {
            Debug.Log($"called assembly scan for type '{typeof(T).FullName}\".");
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Select(t => (Activator.CreateInstance(t) as T)!).ToArray();
        }
    }
}
