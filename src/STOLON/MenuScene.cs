using Betwixt;
using Autofac;

namespace STOLON
{
    public class MenuOrderContainer : OrderContainer<TextElement>
    {
        private bool _capitalize = true;
        private Vector2 _origin;
        private readonly IInputManager _input;
        private readonly Font2D _font;

        public MenuOrderContainer(IInputManager input, Font2D font, IEnumerable<TextElement> elements, Vector2? position = null) : base(input, elements, position, backElementFactory: parentId => TextElement.GetDefaultBackElement(font, parentId))
        {
            _input = input;
            _font = font;
        }

        public override void PrepareOrdering(Vector2 origin, int elementCount) => _origin = origin;

        public override UIElementDrawData GetDrawData(TextElement element, int index, out bool isHovered)
        {
            static char GetDecorator(string elementId, bool isPrefix)
            {
                return isPrefix ? (elementId switch
                {
                    "quit" => 'x',
                    "specialThanks" => '!',
                    _ => '>',
                }) : (elementId switch
                {
                    "quit" => 'x',
                    "specialThanks" => '!',
                    _ => '<',
                });
            }

            Vector2 elementPos = Centering.CenterX((int)element.Font.FastMeasure(element.Text).X,
                                index * (-element.Font.Dimensions.Y * 2 - 2) + _origin.Y,
                                STOLON.VWidth, Vector2.One);
            NumberHelper.OnPixel(ref elementPos);

            Rectangle elementBounds = new Rectangle(elementPos.ToPoint(), new Point((int)element.Font.FastMeasure(element.Text).X, (int)element.Font.Dimensions.Y));
            isHovered = elementBounds.Contains(_input.Mouse.Position);

            StringBuilder elementSb = new StringBuilder(element.Text.Length + 8);

            if (isHovered)
            {
                elementSb.Append(GetDecorator(element.Id, true));
                elementSb.Append(' ');
            }

            elementSb.Append(_capitalize ? element.Text.ToUpper() : element.Text);

            if (isHovered)
            {
                elementSb.Append(' ');
                elementSb.Append(GetDecorator(element.Id, false));
            }

            return new UIElementDrawData(element, elementSb.ToString(), element.Font, element.Type, elementPos + (isHovered ? new Point(-(int)element.Font.FastMeasure(2).X, 0) : Point.Zero).ToVector2(), Rectangle.Empty, false);
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

        private OrderContainer<TextElement> _mainOrderContainer;

        private Tweener<float> _logoEaseTweener;
        private Tweener<float> _removeTweener;

        private Vector2 _splashTextPos;
        private string _splashText;
        private Action? _onLeave;

        private const int LogoRowAmount = 5;

        private readonly ICachedAudioResourceCollection _audio;
        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;
        private readonly IAudioEngine _audioEngine;
        private readonly IConfiguration _config;
        private readonly Environment _environment;
        private readonly IRichLogger _logger;
        private readonly ITaskHeap _tasks;
        private readonly ISceneManager _sceneManager;
        private readonly ITextframe _textframe;
        private readonly IInputManager _input;

        public MenuScene(
            IRichLogger logger,
            ITexture2DCollection textures,
            ICachedAudioResourceCollection audio,
            IFont2DCollection fonts,
            IConfiguration config,
            Environment environment,
            IAudioEngine audioEngine,
            ISceneManager sceneManager,
            ITaskHeap tasks,
            ITextframe textframe,
            IInputManager input,
            IEnumerable<EntityDefinition> entityDefinitions) : base("main_menu")
        {
            _logger = logger;
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

            _logoFlashTime = 0;
            _flashStart = null;
            _logoRowsHidden = 5;
            _ditherTexturePositions = Array.Empty<Point>();

            _depthPath = new List<UIElement>();

            _showSplashtexts = _config.GetBool("graphics.splashtexts_show");
            _showEntityProfiles = _config.GetBool("graphics.entities_show_on_menu");

            _entityProfiles = [entityDefinitions.First().Profile, entityDefinitions.Last().Profile];

            _mainOrderContainer = new MenuOrderContainer(_input, _fonts.Medium, [
                new TextElement("story_start",  "Story", font: _fonts.Medium, clickSound: _audio["exit_3"]),
                new TextElement("com_start","COM",font: _fonts.Medium, clickSound: _audio["coin_4"]),
                new TextElement("xp_start", "2P", font: _fonts.Medium, clickSound: _audio["coin_4"]),
                new TextElement("options","Options", font: _fonts.Medium),
                new TextElement("special_thanks", "Special Thanks", font: _fonts.Medium),
                new TextElement("quit", "Quit", font: _fonts.Medium),
                new TextElement("sound", "Sound", parentId: "options", font: _fonts.Medium),
                new TextElement("graphics", "Graphics", parentId: "options",  font: _fonts.Medium, clickSound: _audio["exit_3"]),
                new TextElement("vol_up",  "Volume UP", parentId: "sound", font: _fonts.Medium),
                new TextElement("vol_down", "Volume DOWN", parentId: "sound",font: _fonts.Medium),
            ]);

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


            _splashText = STOLON.GetSplashText();
        }

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
            int rowHeight = (int)(_logoLines.Height / (float)LogoRowAmount);
            float menuRemoveTweenerOffset = -300f * _removeTweener.Value;
            int lineFromMid = (int)(170f - menuRemoveTweenerOffset);
            bool menuFlashEnded = _millisecondsSinceStartup > _flashEnd;
            int uiElementOffsetY = (int)(280f + menuRemoveTweenerOffset);
            int logoYoffset = (int)(512 - 30 - _logoLines.Height + 8f * _logoEaseTweener.Value * (1 - _removeTweener.Value));
            int logoYScreenCenter = (int)Centering.CenterY(_logoLines, 0, STOLON.VHeight).Y;
            logoYoffset -= (int)((logoYoffset - logoYScreenCenter) * _removeTweener.Value);
            const int MENU_LOGO_BOUNDS_CLEARING = 8;

            //switch (_sceneManager.SkipTarget)
            //{
            //    case "shell":
            //        if (_millisecondsSinceStartup < 10000)
            //        {
            //            _millisecondsSinceStartup = 10001;
            //            _done = true;
            //            _removeTweener.Update(10);
            //            //_boardEntities = [new UserMoveProvider(_input, null, "player0"), new UserMoveProvider(_input, null, "player1")];
            //            Leave();
            //        }
            //        break;
            //}
            //if (_sceneManager.ShouldSkipAnimation(this) && _millisecondsSinceStartup < 10000) _millisecondsSinceStartup = 10001;

            #region inFlash
            _millisecondsSinceStartup += elapsedMilliseconds;
            _logoDrawPos = Vector2.Round(Centering.CenterX(_logoLines, logoYoffset, STOLON.VWidth));
            _logoTileHider = new Rectangle(
                _logoDrawPos.ToPoint() + new Point(0, (int)(rowHeight * (LogoRowAmount - _logoRowsHidden))),
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

            if (_mainOrderContainer.UpdateData["xp_start"].IsClicked(_input))
            {
                //_boardEntities = [new UserMoveProvider(_input, null, "player0"), new UserMoveProvider(_input, null, "player1")];
                Leave();
            }
            if (_mainOrderContainer.UpdateData["vol_up"].IsClicked(_input))
            {
                _audioEngine.MasterVolume += 0.1001f;
                _logger.Log("new volume: " + _audioEngine.MasterVolume);
            }
            if (_mainOrderContainer.UpdateData["vol_down"].IsClicked(_input))
            {
                _audioEngine.MasterVolume -= 0.1001f;
                _logger.Log("new volume: " + _audioEngine.MasterVolume);
            }
            if (_mainOrderContainer.UpdateData["story_start"].IsClicked(_input))
            {
                _textframe.Queue(new DialogueInfo(_environment, "Not yet implemented."));
            }
            if (_mainOrderContainer.UpdateData["com_start"].IsClicked(_input))
            {
                //_boardEntities = [new UserMoveProvider(_input, null, "player0"), _environment.Entities["goldsilk"]];
                Leave();
            }
            if (_mainOrderContainer.UpdateData["special_thanks"].IsClicked(_input))
            {
                _textframe.Queue(new DialogueInfo(_environment, "Please read the github README."));
            }
            if (_mainOrderContainer.UpdateData["quit"].IsClicked(_input))
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
            }), 2000, false);
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
                drawingContext.DrawRectangle(_logoBoundingBox, Color.White, 2);

            }
            if (_drawLogoDummyTiles) drawingContext.Draw(_logoMarks, _logoDrawPos);
            if (_drawLogoFilledTiles) drawingContext.Draw(_logoFilledMarks, _logoDrawPos);
            if (_drawLogoLowResFonted) drawingContext.Draw(_logoFonted, _logoDrawPos);

            drawingContext.DrawArea(_logoTileHider, Color.Black);
            if (_drawLogoLines) drawingContext.Draw(_logoLines, _logoDrawPos);

            drawingContext.DrawLine(RemoveLine1x, STOLON.VHeight, RemoveLine1x, _removeLineYAmount, Color.White, 2);
            drawingContext.DrawLine(RemoveLine2x, STOLON.VHeight, RemoveLine2x, _removeLineYAmount, Color.White, 2);

            if (_showEntityProfiles)
            {
                drawingContext.DrawEntity(_entityProfiles[0], 512, new Vector2(STOLON.VWidth / 2 + 64, 0), drawMode: EntityDrawMode.Menu);
                drawingContext.DrawEntity(_entityProfiles[1], 512, new Vector2(STOLON.VWidth / 2 - 512, 0), drawMode: EntityDrawMode.Menu);
            }

            _mainOrderContainer.Draw(drawingContext);
        }
    }
}
