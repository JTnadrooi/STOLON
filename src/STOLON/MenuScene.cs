using Betwixt;
using Autofac;

namespace STOLON
{
    public class MenuOrderContainer : OrderContainer
    {
        private bool _capitalize = true;
        private Vector2 _origin;

        private readonly Font2D _font;
        private readonly IInputManager _input;

        public MenuOrderContainer(IInputManager input, IEnumerable<UIElement> elements, Font2D font, Vector2? position = null) : base(input, elements, position)
        {
            _font = font;
            _input = input;
        }

        public override void PrepareOrdering(Vector2 origin, int elementCount) => _origin = origin;

        public override UIElementDrawData GetDrawData(UIElement element, int index, out bool isHovered)
        {
            Vector2 elementPos = Centering.CenterX((int)_font.FastMeasure(element.Text).X,
                                index * (-_font.Dimensions.Y * 2 - 2) + _origin.Y,
                                STOLON.VWidth, Vector2.One);
            NumberHelper.OnPixel(ref elementPos);

            Rectangle elementBounds = new Rectangle(elementPos.ToPoint(), new Point((int)_font.FastMeasure(element.Text).X, (int)_font.Dimensions.Y));
            string elementText = element.Text;
            if (_capitalize) elementText = elementText.ToUpper();

            string postPre = element.Id switch
            {
                "quit" => "x",
                "specialThanks" => "!",
                _ => ">",
            };
            isHovered = elementBounds.Contains(_input.Mouse.Position);
            return new UIElementDrawData(element, isHovered
                ? (postPre + " " + elementText + " " + postPre.Replace(">", "<"))
                : elementText, _font, element.Type, elementPos + (isHovered ? new Point(-(int)_font.FastMeasure(2).X, 0) : Point.Zero).ToVector2(), Rectangle.Empty, false);
        }
    }
    public class MenuScene : Scene
    {
        private Texture2D _logoLines;
        private Texture2D _logoMarks;
        private Texture2D _logoFilledMarks;
        private Texture2D _logoFonted;
        private Texture2D _dither32;

        private EntityProfile[] _entityProfiles;

        private Rectangle _logoTileHider;

        private bool _drawLogoLines;
        private bool _drawLogoDummyTiles;
        private bool _drawLogoFilledTiles;
        private bool _drawLogoLowResFonted;
        private int _logoRowsHidden;
        private Rectangle _logoBoundingBox;

        private Vector2 _logoDrawPos;
        private int _millisecondsSinceStartup;

        private int _logoFlashTime;
        private int? _flashStart;
        private int? _flashEnd;
        private int _millisecondsFlashing;
        private int _millisecondsSinceMenuRemoveStart;
        private bool _done;

        private int _divLine1X;
        private int _divLine2X;
        private int _divLineLength;
        private int _divLineWidth;

        private int _removeLineYAmount;

        internal int RemoveLine1x;
        internal int RemoveLine2x;

        private bool _showSplashtexts;
        private bool _showEntityProfiles;

        private List<UIElement> _depthPath;

        private Point[] _ditherTexturePositions;

        private OrderContainer _mainOrderContainer;

        private Tweener<float> _logoEaseTweener;
        private Tweener<float> _removeTweener;

        private string[] _splashTexts;
        private Vector2 _splashTextPos;
        private string _splashText;
        private Action? _onLeave;
        private bool _fastLeave;

        private const int LOGO_ROW_COUNT = 5;
        private Player[]? _boardPlayers;

        private readonly ICachedAudioResourceCollection _audio;
        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;
        private readonly IAudioEngine _audioEngine;
        private readonly IConfiguration _config;
        private readonly Environment _environment;
        private readonly IRichLogger _logger;
        private readonly Interface _ui;
        private readonly ITaskHeap _tasks;
        private readonly ISceneManager _sceneManager;
        private readonly ITextframe _textframe;
        private readonly IInputManager _input;

        public MenuScene(
            IRichLogger logger,
            Interface ui,
            ITexture2DCollection textures,
            ICachedAudioResourceCollection audio,
            IFont2DCollection fonts,
            IConfiguration config,
            Environment environment,
            IAudioEngine audioEngine,
            ISceneManager sceneManager,
            ITaskHeap tasks,
            ITextframe textframe,
            IInputManager input) : base("main_menu")
        {
            _logger = logger;
            _ui = ui;
            _audio = audio;
            _textures = textures;
            _fonts = fonts;
            _audioEngine = audioEngine;
            _config = config;
            _environment = environment;
            _tasks = tasks;
            _sceneManager = sceneManager;
            _textframe = textframe;
            _input = input;

            _logoLines = _textures.GetReference("UI\\Logo\\Menu\\lines");
            _logoMarks = _textures.GetReference("UI\\Logo\\Menu\\marks");
            _logoFilledMarks = _textures.GetReference("UI\\Logo\\Menu\\filled_marks");
            _logoFonted = _textures.GetReference("UI\\Logo\\Menu\\fonted");
            _dither32 = _textures.GetReference("dither-32");
            _drawLogoLines = true;
            _drawLogoDummyTiles = true;
            _drawLogoFilledTiles = false;
            _fastLeave = false;

            _logoFlashTime = 0;
            _flashStart = null;
            _logoRowsHidden = 5;
            _ditherTexturePositions = Array.Empty<Point>();

            _depthPath = new List<UIElement>();

            _showSplashtexts = _config.GetBool("graphics.splashtexts_show");
            _showEntityProfiles = _config.GetBool("graphics.entities_show_on_menu");

            _entityProfiles = [_environment.Entities.Values.First().Profile, _environment.Entities.Values.Last().Profile];

            _mainOrderContainer = new MenuOrderContainer(_input, [
                new UIElement("story_start", UIElement.TopId, "Story", UIElementType.Listen, clickSound: _audio["exit_3"]),
                new UIElement("com_start", UIElement.TopId, "COM", UIElementType.Listen, clickSound: _audio["coin_4"]),
                new UIElement("xp_start", UIElement.TopId, "2P", UIElementType.Listen, clickSound: _audio["coin_4"]),
                new UIElement("options", UIElement.TopId, "Options", UIElementType.Listen),
                new UIElement("special_thanks", UIElement.TopId, "Special Thanks", UIElementType.Listen),
                new UIElement("quit", UIElement.TopId, "Quit", UIElementType.Listen),
                new UIElement("sound", "options", "Sound", UIElementType.Listen),
                new UIElement("graphics", "options", "Graphics", UIElementType.Listen, clickSound: _audio["exit_3"]),
                new UIElement("vol_up", "sound", "Volume UP", UIElementType.Listen),
                new UIElement("vol_down", "sound", "Volume DOWN", UIElementType.Listen),
            ], _fonts.Medium);

            //switch (_skipTo)
            //{
            //    case "main_menu":
            //        _skipLogoAnimation = true;
            //        break;
            //    case "entity_select":
            //        _skipLogoAnimation = true;
            //        break;
            //}


            _logoEaseTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);
            _removeTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);

            //Console.WriteLine(_ui.UIElements.ToJoinedString(", "));

            _splashTexts = new string[]
            {
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
                "\"Is that an ability or a program?\"", // Nadrooi quote.
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
            };

            _splashText = _splashTexts[new Random().Next(0, _splashTexts.Length)];
        }

        /// <summary>
        /// Get random splash text.
        /// </summary>
        /// <returns>A random splash text.</returns>
        public string GetRandomSplashText() => _splashTexts[new Random().Next(0, _splashTexts.Length)];
        /// <summary>
        /// Get a random splash text and get the <paramref name="i"/> as index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetRandomSplashText(out int i) => _splashTexts[i = new Random().Next(0, _splashTexts.Length)];
        /// <summary>
        /// Leave the main menu.
        /// </summary>
        public void Leave(Action? onLeave = null)
        {
            _done = true;
            this._onLeave = onLeave;
        }
        protected override void UpdateInterface(int elapsedMilliseconds)
        {
            int rowHeight = (int)(_logoLines.Height / (float)LOGO_ROW_COUNT);
            float menuRemoveTweenerOffset = -300f * _removeTweener.Value;
            int lineFromMid = (int)(170f - menuRemoveTweenerOffset);
            bool menuFlashEnded = _millisecondsSinceStartup > _flashEnd;
            int uiElementOffsetY = (int)(280f + menuRemoveTweenerOffset);
            int logoYoffset = (int)(512 - 30 - _logoLines.Height + 8f * _logoEaseTweener.Value * (1 - _removeTweener.Value));
            int logoYScreenCenter = (int)Centering.CenterY(_logoLines, 0, STOLON.VHeight).Y;
            logoYoffset -= (int)((logoYoffset - logoYScreenCenter) * _removeTweener.Value);
            const int MENU_LOGO_BOUNDS_CLEARING = 8;

            switch (Scene.SkipTarget)
            {
                case "shell":
                    if (_millisecondsSinceStartup < 10000)
                    {
                        _millisecondsSinceStartup = 10001;
                        _done = true;
                        _removeTweener.Update(10);
                        _boardPlayers = [new Player("player0"), new Player("player1")];
                        _fastLeave = true;
                        Leave();
                    }
                    break;
            }
            if (ShouldSkipAnimation() && _millisecondsSinceStartup < 10000) _millisecondsSinceStartup = 10001;

            #region inFlash
            _millisecondsSinceStartup += elapsedMilliseconds;
            _logoDrawPos = Vector2.Round(Centering.CenterX(_logoLines, logoYoffset, STOLON.VWidth));
            _logoTileHider = new Rectangle(
                _logoDrawPos.ToPoint() + new Point(0, (int)(rowHeight * (LOGO_ROW_COUNT - _logoRowsHidden))),
                new Point((int)(_logoLines.Width), (int)(rowHeight * _logoRowsHidden))
            );

            _flashStart = 1200;
            _flashEnd = _flashStart + 400;

            _divLine1X = (int)(STOLON.VWidth / 2f) - lineFromMid;
            _divLine2X = (int)(STOLON.VWidth / 2f) + lineFromMid;

            _divLineLength = _drawLogoFilledTiles ? STOLON.VHeight : 0;
            _divLineWidth = 2 + (menuFlashEnded ? 2 : 0);

            if (_millisecondsSinceStartup > 300) _logoRowsHidden = 4;
            if (_millisecondsSinceStartup > 600) _logoRowsHidden = 3;
            if (_millisecondsSinceStartup > 800) _logoRowsHidden = 2;
            if (_millisecondsSinceStartup > 1000) _logoRowsHidden = 1;
            if (_millisecondsSinceStartup > _flashStart) _logoRowsHidden = 0;

            if (_logoFlashTime > 0)
            {
                _millisecondsFlashing += elapsedMilliseconds;
                if (_millisecondsFlashing > _logoFlashTime)
                {
                    _drawLogoFilledTiles = !_drawLogoFilledTiles;
                    _millisecondsFlashing = 0;
                }
            }

            if (!_flashStart.HasValue) return; // code below only relevant when the dummy board show animation ended.

            if (_millisecondsSinceStartup > _flashStart.Value) _logoFlashTime = 120;
            if (_millisecondsSinceStartup > _flashStart.Value + 200) _logoFlashTime = 100;
            if (_millisecondsSinceStartup > _flashStart.Value + 300) _logoFlashTime = 75;
            if (_millisecondsSinceStartup > _flashStart.Value + 350) _logoFlashTime = 60;
            if (_millisecondsSinceStartup < _flashEnd) return; // code below only relevant when the full animation ended.

            #endregion
            #region inMenu
            _drawLogoLowResFonted = true;
            _drawLogoDummyTiles = true;
            _drawLogoFilledTiles = true;
            _drawLogoLines = true;

            _logoFlashTime = 0; // ensures disabled flashing.
            _logoEaseTweener.Update(elapsedMilliseconds / 1000f); // update the tweener.
            if (_logoEaseTweener.Value == 1 || _logoEaseTweener.Value == 0) // reverse and restart if finished.
            {
                _logoEaseTweener.Reverse();
                _logoEaseTweener.Start();
                _logger.Log("reversed icon tweener.");
            }
            _logoBoundingBox =
                new Rectangle(_logoDrawPos.ToPoint() + new Point(-MENU_LOGO_BOUNDS_CLEARING), _logoLines.Bounds.Size + new Point(MENU_LOGO_BOUNDS_CLEARING * 2));

            _ditherTexturePositions = new Point[(int)Math.Ceiling(STOLON.VHeight / (float)_dither32.Height) * 2];
            for (int i = 0; i < _ditherTexturePositions.Length; i++) // dithering positions.
                _ditherTexturePositions[i] = new Point(
                        (i >= _ditherTexturePositions.Length / 2f) ? _divLine2X : _divLine1X - _dither32.Width,
                        (i % (int)(_ditherTexturePositions.Length / 2f)) * _dither32.Height);

            _mainOrderContainer.Position = new Vector2(0, uiElementOffsetY);
            _mainOrderContainer.Update(elapsedMilliseconds);
            //UIOrdering.Order(_ui.Elements.Values.ToArray(), _ui.MenuPath, _ui.DrawData, _ui.UpdateData, , );

            if (_ui.UpdateDump["xp_start"].IsClicked(_input))
            {
                _boardPlayers = [new Player("player0"), new Player("player1")];
                Leave();
            }
            if (_ui.UpdateDump["vol_up"].IsClicked(_input))
            {
                _audioEngine.MasterVolume += 0.1001f;
                _logger.Log("new volume: " + _audioEngine.MasterVolume);
            }
            if (_ui.UpdateDump["vol_down"].IsClicked(_input))
            {
                _audioEngine.MasterVolume -= 0.1001f;
                _logger.Log("new volume: " + _audioEngine.MasterVolume);
            }
            if (_ui.UpdateDump["story_start"].IsClicked(_input))
            {
                _textframe.Queue(new DialogueInfo(_environment, "Not yet implemented."));
            }
            if (_ui.UpdateDump["com_start"].IsClicked(_input))
            {
                _boardPlayers = [new Player("player0"), _environment.Entities["goldsilk"].GetPlayer()];
                Leave();
            }
            if (_ui.UpdateDump["special_thanks"].IsClicked(_input))
            {
                _textframe.Queue(new DialogueInfo(_environment, "Please read the github README."));
            }
            if (_ui.UpdateDump["quit"].IsClicked(_input))
            {
                STOLON.Instance.Exit();
            }

            if (!_done) return;
            #endregion

            _removeTweener.Update(elapsedMilliseconds / 1000f);
            _tasks.SafePush("menu_logo_disapear", new DynamicTask(() => // fire and forget game logic ftw
            {
                _onLeave?.Invoke();
                _onLeave = null;
                //STOLON.StateManager.ChangeState<BoardGameState>(true);
                //((BoardGameState)STOLON.StateManager.Current).SetBoard(_boardPlayers!);
                _sceneManager.ChangeScene<ShellScene>();
                _boardPlayers = null;
            }), _fastLeave ? 10 : 2000, false);
            _millisecondsSinceMenuRemoveStart += elapsedMilliseconds;

            _splashTextPos = Centering.CenterX((int)(_fonts.Small.FastMeasure(_splashText).X),
                _logoDrawPos.Y - _fonts.Small.Dimensions.Y - (MENU_LOGO_BOUNDS_CLEARING * Math.Clamp(_removeTweener.Value * 2f, 0f, 1f)), STOLON.VWidth, Vector2.One);

            _removeLineYAmount = STOLON.VHeight - (int)(_removeTweener.Value * STOLON.VHeight);
            int lDelta = (int)(_logoDrawPos.X - 8);
            RemoveLine1x = lDelta;
            RemoveLine2x = STOLON.VWidth - lDelta;

            NumberHelper.OnPixel(ref _logoDrawPos);
        }
        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawLine(_divLine1X, -10f, _divLine1X, _divLineLength, Color.White, _divLineWidth);
            drawingContext.DrawLine(_divLine2X, -10f, _divLine2X, _divLineLength, Color.White, _divLineWidth);
            if (_done && _showSplashtexts) drawingContext.DrawString(_fonts.Small, _splashText, _splashTextPos);

            if (_drawLogoLowResFonted)
            {
                for (int i = 0; i < _ditherTexturePositions.Length; i++)
                    drawingContext.Draw(_dither32, _ditherTexturePositions[i].ToVector2(), effects: (i >= _ditherTexturePositions.Length / 2f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

                drawingContext.DrawArea(_logoBoundingBox, Color.Black);
                drawingContext.DrawRectangle(_logoBoundingBox, Color.White, Interface.LineWidth);

            }
            if (_drawLogoDummyTiles) drawingContext.Draw(_logoMarks, _logoDrawPos);
            if (_drawLogoFilledTiles) drawingContext.Draw(_logoFilledMarks, _logoDrawPos);
            if (_drawLogoLowResFonted) drawingContext.Draw(_logoFonted, _logoDrawPos);

            drawingContext.DrawArea(_logoTileHider, Color.Black);
            if (_drawLogoLines) drawingContext.Draw(_logoLines, _logoDrawPos);

            drawingContext.DrawLine(RemoveLine1x, STOLON.VHeight, RemoveLine1x, _removeLineYAmount, Color.White, Interface.LineWidth);
            drawingContext.DrawLine(RemoveLine2x, STOLON.VHeight, RemoveLine2x, _removeLineYAmount, Color.White, Interface.LineWidth);

            if (_showEntityProfiles)
            {
                drawingContext.DrawEntity(_entityProfiles[0], 512, new Vector2(STOLON.VWidth / 2 + 64, 0), drawMode: EntityDrawMode.Menu);
                drawingContext.DrawEntity(_entityProfiles[1], 512, new Vector2(STOLON.VWidth / 2 - 512, 0), drawMode: EntityDrawMode.Menu);
            }

            _mainOrderContainer.Draw(drawingContext);
        }
    }
}
