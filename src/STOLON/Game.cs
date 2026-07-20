using Autofac;
using DiscordRPC.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public sealed class STOLON : Game
    {
        private readonly IRichLogger _logger;
        private readonly IInputManager _input;
        private readonly ITaskHeap _tasks;
        private readonly IConfiguration _config;
        private readonly Lazy<Environment> _environment;
        private readonly Lazy<DiscordRichPresence> _drp;
        private readonly Lazy<IEnumerable<IResourceCollection>> _resourceCollections;
        private readonly Lazy<FoliageEngine> _foliageEngine;
        private readonly Lazy<DrawingContext> _drawingContext;

        public Point DesiredDimensions => new Point(AspectRatioX * _desiredModifier, AspectRatioY * _desiredModifier);
        public Point ScreenCenter => new Point(VWidth / 2, VHeight / 2);
        public float ScreenScale { get; private set; }

        public GraphicsDeviceManager GraphicsDeviceManager => _graphics;

        private Foliage? _topFoliage;
        private GraphicsDeviceManager _graphics;
        private int _desiredModifier;
        private Point _oldWindowSize;

        public STOLON(IRichLogger logger,
            IInputManager input,
            ITaskHeap tasks,
            IConfiguration config,
            Lazy<Environment> environment,
            Lazy<DiscordRichPresence> drp,
            Lazy<IEnumerable<IResourceCollection>> resourceCollections,
            Lazy<FoliageEngine> foliageEngine,
            Lazy<DrawingContext> drawingContext)
        {
            _logger = logger;
            _input = input;
            _tasks = tasks;
            _config = config;
            _environment = environment;
            _drp = drp;
            _resourceCollections = resourceCollections;
            _foliageEngine = foliageEngine;
            _drawingContext = drawingContext;

            _instance = this;

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = string.Empty; // heh
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _drp.Value.Initialize();

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

            _drawingContext.Value.UpdateResolution();
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
            foreach (IResourceCollection resourceCollection in _resourceCollections.Value)
            {
                if (resourceCollection.IsLoaded) continue;

                resourceCollection.LoadResources();
                loadCount++;
            }

            _environment.Value.Initialize();
            _topFoliage = new Foliage(_foliageEngine.Value, Line.CreateHorizontal(0, STOLON.VWidth, STOLON.VHeight), maxReachFunction: f => (int)(f * STOLON.VHeight + 20), drawCorners: true);

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
                _environment.Value.Update(elapsedMilliseconds);

                _input.PostUpdate(elapsedMilliseconds);

                if (_input.IsPressed(Keys.LeftControl))
                {
                    if (_input.IsClicked(Keys.F)) GoFullscreen();
                    if (_input.IsClicked(Keys.S)) _drawingContext.Value.Screenshot();
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (IsActive)
            {
                _drawingContext.Value.BeginScene();

                _environment.Value.Draw(_drawingContext.Value);
                //_drawingContext.DrawString(_fonts.Small, Version, new Vector2(V_WIDTH / 2 - _fonts.Small.FastMeasure(Version).X / 2, 500));
                _drawingContext.Value.DrawRectangle(STOLON.Instance.GetVirtualBounds(), Color.White, 1);
                _topFoliage!.Draw(_drawingContext.Value);

                _drawingContext.Value.EndScene();
            }

            base.Draw(gameTime);
        }

        public static readonly Rectangle Bounds = new Rectangle(0, 0, VWidth, VHeight);
        public static Color Color1 => new Color(242, 251, 235); // #f2fbeb
        public static Color Color2 => new Color(23, 18, 25); // #171219
        public static string Version { get; } = File.ReadAllText(".version");
        public static bool IsInitiated => _instance is not null;
        public static STOLON Instance => _instance ?? throw new InvalidOperationException("STOLON is not initiated.");
        public static ReadOnlyCollection<string> SplashTexts { get; } = new ReadOnlyCollection<string>([
                "The center rows are most valueable.", // fact, the tiles in them have the most posibilies.
                "CENTER, ROWS, VALUABLE.",
                "They are stingers.", // Thetalore Fax(char) reference.
                "STOLON's deadline has always been 2025.", // Uh oh. 10/12/2025
                "If you listen very closely you can hear the main theme.",
                "If you listen very closely you can hear the sound effects.",
                "Listed twice.",
                "KEES NOOOOOOOO", // Keespro reference.
                "That definitely something Vox would say.", // Voxuuu reference.
                "Inity waits patiently..", // Initial3d waiting for art reference.
                "Super colliding..", // LandronSC/lanpi (dicord user) reference.
                "Teaching garden chairs how to fly..", // FlyingGarderChair (dicord user) reference.
                "Oh dear..",
                "Goldsilk hates the player.",
                "This week.",
                "Good luck.",
                "Good luck!",
                "Good luck!!",
                "This is a fake loading screen.",
                "This is a real loading screen.",
                "For Them, Light.", // Thetalore reference.
                "Can you read this?",
                "CAN YOU READ THIS?",
                "POWER SURGING!", // Megumin reference.
                "There,", // Thetalore reference.
                "No shaders?",
                "All colors, Her.", // Thetalore Nue reference. (yeah i like these kind of sentences)
                "Thanks for playing! :D",
                //"\"Call that a Natural Deadline.\"",
                "Nue not included!",
                "Fishing update when?",
                "\"What even is a Stolon?\"", // stolons are some sort of tree "root". 
                "The Sun is gone..", // Terraria mod reference.
                "Comparing chaos to disorder..",
                "Luck good.",
                "The chance of getting this message is quite low.",
                "Fax as in the machine.", // fax (thetalore char) reference.
                "Self proclaimed..?",
                "The Musical",
                "The Movie",
                "Why is Lanulox here..",
                "Time's Up! Fate sealed.", // Thetalore Nue reference.
                "Seems vacant..", // inside joke around the word "vacant".
                "You are week, I am month.", // meme reference.
                "Lanu Lanu Lanu La-", // Lanulox reference
                "Welcome.",
                "Welcome!",
                "Galore.", // fav word.
                "Galore!",
                "NOT solved.",
                "NOT CLUELESS!", // prof dave explains reference. (from debate against tour)
                "27 Compile errors..?", // reference to cracktorio finding out STOLON only builds on my pc. (fixed now)
                "Simply Rendering,",
                "Behold, The \"Sky Train\"!", // reference to one of my Stormworks creations.
                "dot hat :drool:",
                "Cherry-pilled!", // Cherry (lanpi) reference.
                "The Stolons brace themselfs..", // Motorstorm reference.
                "Potatofruit?", // Thetalore reference.
                "A reality loved by many, hated by more.", // Thetalore quote.
                "VWS cares not.",
                "Eeeeh maji? Easy modo???", // Touhou reference.
                "Sto owes someone 5 dollars.", // Superman 5 dollars meme reference.
                "\"Souls are overrated but quite underused.\"", // Thetalore quote.
                "1bit!", // I suppose STOLON isnt 1 bit anymore.
                "haha", // Bloem reference.
                "ma'am", // Bloem reference.
                "elevenhundredthousand.",
                "The comfort of finity.", // Antics (lanpi) reference
                "Pressure discrepancy detected - reversing airflow.", // White knuckle reference.
                ":LOVINGSTARE:", // Efvour reference. (STOLON character)
                ":STARE:", // Efvour reference. (STOLON character)
                "Collida past 3.", // Lanpi reference.
                "That translates to \"flour\".", // Bloem reference.
                "Index is jealous.",
                "the chairs have eyes",
                "\"Its funny. You.\"", // Efvour talks like this.
                "The BOULDER.", // that one cavevideo meme maker.
                "Seven-eyed wonders.", // Cenci reference.
                "Antartica is not the answer.", // random meme about people going to antartica as escape from life for some reason.
                "Alloclassified.", // Alloclasse reference. (STOLON character)
                "Envi states but doesn't inform.", // im trying to make envi helpfull...
                "Powered by AsitLib!", // STOLON makes heavy use of one of my libaries named AsitLib.
                "Powered by AsitLib's mild enthusiasm!",
                "\"Guys.. Guys.. I think this game was made by ONLY ONE DEVELOPER!?!!111!", // reference to a roblox horror game gameplay video (to long probably)
                "Christmass special!",
                "ITS BLUE! ITS BLUE!", // limbo verification run.
                "FOCUS", // limbo.
                "l'n'p's", // lanpi.
                "A vague sense of purpose.",
                "Nanoda!", // kemono friends.
                "Beste reizigers,", // NS (Dutch railways thing).
                //"Unintended but full of intent.", // Thetalore quote.
                "The stolons seem reluctant.",
                "JAN43", // Inside joke.
                "Drop asimetrico. Preparate!", // Duelo Maestro gd level.
                "You have been Noticed.",
                "I put that there.",
                "Powered by hopes and whimsy.",
                "Dual warning!", // reference to one of my geometry dash levels.
                "Your world has been blessed with cobalt!", // terraria reference.
                "You have been Noticed.",
                "Drawing vegitation..",
                "FLORA.",
                "Stukadoor", // internship joke.
                "The rest of your life is probably a loooong time..",
                "Woo! Nyaa!", // Haiyore! Nyaruko-san reference
                "Fuwa Fuwa Fuwa Fuwa", // Princess Advent reference (D4DJ)
                "Marvelous~!", // Marvelous sunday reference (Uma musume)
                "Are you sure whatever you're doing is worth it?", // subnautica reference
        ]);

        private static STOLON? _instance;
        private static IContainer? _services;

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
        }

        public static string GetSplashText() => GetSplashText(out _);
        public static string GetSplashText(out int chosenIndex)
        {
            chosenIndex = new Random().Next(0, SplashTexts.Count);

            return SplashTexts[chosenIndex];
        }

        public const int VWidth = AspectRatioX * VirtualModifier;
        public const int VHeight = AspectRatioY * VirtualModifier;
        public const int AspectRatioX = 16;
        public const int AspectRatioY = 9;
        public const int VirtualModifier = 57;
        public const float AspectRatioFloat = AspectRatioX / (float)AspectRatioY;
    }
}
