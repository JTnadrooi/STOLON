using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Point = Microsoft.Xna.Framework.Point;
using Microsoft.Xna.Framework.Content;
using Betwixt;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using static STOLON.UIElement;

namespace STOLON
{
    public class MenuGameState : GameState
    {
        private GameTexture _menuLogoLines;
        private GameTexture _menuLogoMarks;
        private GameTexture _menuLogoFilledMarks;
        private GameTexture _menuLogoFonted;
        private GameTexture _dither32;

        private Rectangle _menuLogoTileHider;

        private bool _drawMenuLogoLines;
        private bool _drawMenuLogoDummyTiles;
        private bool _drawMenuLogoFilledTiles;
        private bool _drawMenuLogoLowResFonted;
        private int _menuLogoRowsHidden;
        private Rectangle _menuLogoBoundingBox;

        private Vector2 _menuLogoDrawPos;
        private int _milisecondsSinceStartup;

        private int _menuLogoFlashTime;
        private int? _menuFlashStart;
        private int? _menuFlashEnd;
        private int _menuLogoMilisecondsFlashing;
        private int _milisecondsSinceMenuRemoveStart;
        private bool _menuDone;

        private int _menuLine1X;
        private int _menuLine2X;
        private int _menuLineLenght;
        private int _menuLineWidth;

        private int _menuRemoveLineY;
        internal int MenuRemoveLine1x;
        internal int MenuRemoveLine2x;

        private string _skipTo;
        private bool _skipAnimation;
        private bool _showSplashtexts;
        private bool _showEntityProfiles;

        private List<UIElement> _depthPath;

        private Point[] _menuDitherTexturePositions;
        private IReadOnlyDictionary<string, EntityProfile> _entityProfiles;

        private Tweener<float> _menuLogoEaseTweener;
        private Tweener<float> _menuRemoveTweener;

        private string[] _tips;
        private Vector2 _tipPos;
        private int _tipId;
        private Action? _onLeave;

        private const int MENU_LOGO_ROW_COUNT = 5;
        private Player[]? _boardPlayers;

        public MenuGameState() : base("main_menu")
        {
            _menuLogoLines = STOLON.Textures.GetReference("Logo\\Menu\\lines");
            _menuLogoMarks = STOLON.Textures.GetReference("Logo\\Menu\\marks");
            _menuLogoFilledMarks = STOLON.Textures.GetReference("Logo\\Menu\\filled_marks");
            _menuLogoFonted = STOLON.Textures.GetReference("Logo\\Menu\\fonted");
            _dither32 = STOLON.Textures.GetReference("dither-32");
            _drawMenuLogoLines = true;
            _drawMenuLogoDummyTiles = true;
            _drawMenuLogoFilledTiles = false;

            _entityProfiles = STOLON.Environment.GetEntityProfiles();

            _menuLogoFlashTime = 0;
            _menuFlashStart = null;
            _menuLogoRowsHidden = 5;
            _menuDitherTexturePositions = Array.Empty<Point>();

            _depthPath = new List<UIElement>();

            _skipTo = STOLON.Config.GetString("Debug.skip_to");
            _skipAnimation = STOLON.Config.GetBool("Debug.skip_gamestage_animation");
            _showSplashtexts = STOLON.Config.GetBool("Graphics.splashtexts_show");
            _showEntityProfiles = STOLON.Config.GetBool("Graphics.entities_show_on_menu");

            //switch (_skipTo)
            //{
            //    case "main_menu":
            //        _skipLogoAnimation = true;
            //        break;
            //    case "entity_select":
            //        _skipLogoAnimation = true;
            //        break;
            //}

            STOLON.UI.AddElement(new UIElement(UserInterface.TITLE_PARENT_ID, UIElement.TOP_ID, string.Empty, UIElementType.Listen));

            STOLON.UI.AddElement(new UIElement("story_start", UserInterface.TITLE_PARENT_ID, "Story", UIElementType.Listen, clickSoundId: "exit3"));
            STOLON.UI.AddElement(new UIElement("com_start", UserInterface.TITLE_PARENT_ID, "COM", UIElementType.Listen, clickSoundId: "coin4"));
            STOLON.UI.AddElement(new UIElement("xp_start", UserInterface.TITLE_PARENT_ID, "2P", UIElementType.Listen, clickSoundId: "coin4"));
            STOLON.UI.AddElement(new UIElement("options", UserInterface.TITLE_PARENT_ID, "Options", UIElementType.Listen));
            STOLON.UI.AddElement(new UIElement("special_thanks", UserInterface.TITLE_PARENT_ID, "Special Thanks", UIElementType.Listen));
            STOLON.UI.AddElement(new UIElement("quit", UserInterface.TITLE_PARENT_ID, "Quit", UIElementType.Listen));

            // options
            STOLON.UI.AddElement(new UIElement("sound", "options", "Sound", UIElementType.Listen));
            STOLON.UI.AddElement(new UIElement("graphics", "options", "Graphics", UIElementType.Listen, clickSoundId: "exit3"));

            STOLON.UI.AddElement(new UIElement("vol_up", "sound", "Volume UP", UIElementType.Listen));
            STOLON.UI.AddElement(new UIElement("vol_down", "sound", "Volume DOWN", UIElementType.Listen));

            STOLON.UI.MenuPath = GetSelfPath(UserInterface.TITLE_PARENT_ID);
            STOLON.Debug.Log(">autogenerating _back_ buttons");
            HashSet<string> parentIds = STOLON.UI.GetParentIds();
            foreach (string id in parentIds) STOLON.UI.AddElement(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));

            _menuLogoEaseTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);
            _menuRemoveTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);

            //Console.WriteLine(STOLON.UI.UIElements.ToJoinedString(", "));

            _tips = new string[]
            {
                //"A Stolon is a line where both players cannot drop their tiles.", // to long
                "The center rows are most valueable.",
                "CENTER, ROWS, VALUABLE.",
                "The border rows are most respectable.",
                "They are stingers.",
                "Reality is overrated.",
                "STOLON's deadline has always been 2025.",
                "If you listen very closely you can hear the main theme.",
                "If you listen very closely you can hear the sound effects.",
                "Listed twice.",
                "KEES NOOOOOOOO",
                "That definitely something Vox would say.",
                "Inity waits patiently..",
                "Super colliding..",
                "Teaching garden chairs how to fly..",
                "\"Is that an ability or a program?\"",
                "Oh dear..",
                "Goldsilk hates the player.",
                "This week.",
                "Good luck.",
                "Good luck!",
                "Good luck!!",
                "This is a fake loading screen.",
                "This is a real loading screen.",
                "For Them, Light.",
                "Can you read this?",
                "CAN YOU READ THIS?",
                "POWER SURGING!",
                "Galore!",
                "The start of the unending.",
                "There,",
                "No shaders?",
                "All colors, Her.",
                "Thanks for playing! :D",
                "\"Call that a Natural Deadline.\"",
                "Nue not included!",
                "Assembling the Pharos..",
                "Fishing update when?",
                "\"What even is a Stolon?\"",
                "The Sun is gone..",
                "Comparing chaos to disorder..",
                "Luck good.",
                "The chance of getting this message is quite low.",
                "Fax as in the machine.",
                "Self proclaimed?.",
                "The Musical",
                "Why is Lanulox here..",
                "Time's Up! Fate sealed.",
                "Seems vacant..",
                "You are week, I am month.",
                "Lanu Lanu Lanu La-",
                "Welcome.",
                "Welcome!",
                "Galore.",
                "NOT solved.",
                "NOT CLUELESS!",
                "Tiory?",
                "27 Compile errors..?",
                "Simply Rendering,",
                "Behold, The \"Sky Train\"!",
                "dot hat :drool:",
                "Cherry-pilled!",
                "The stolons brace themselfs..",
                "Potatofruit?",
                "A reality loved by many, hated by more.",
                "VWS cares not.",
                "Eeeeh maji? Easy modo???",
                "Sto owes someone 5 dollars.",
                "\"Souls are overrated but quite underused.\"",
                "\"I-I don't quite understand..\"",
                "1bit!",
                "haha",
                "elevenhundredthousand.",
                "The comfort of finity.",
                "Pressure discrepancy detected - reversing airflow.",
                ":LOVINGSTARE:",
                ":STARE:",
                "Collida past 3.",
                "That translates to \"flour\".",
                "Index is jealous.",
                "the chairs have eyes",
                "\"Its funny. You.\"",
                "\"Human might be a over-/under- statement, whatever, its never quite right.\"",
            };

            _tipId = new Random().Next(0, _tips.Length);

        }

        /// <summary>
        /// Get random splash text.
        /// </summary>
        /// <returns>A random splash text.</returns>
        public string GetRandomSplashText() => _tips[new Random().Next(0, _tips.Length)];
        /// <summary>
        /// Get a random splash text and get the <paramref name="i"/> as index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetRandomSplashText(out int i) => _tips[i = new Random().Next(0, _tips.Length)];
        /// <summary>
        /// Leave the main menu.
        /// </summary>
        public void Leave(Action? onLeave = null)
        {
            _menuDone = true;
            this._onLeave = onLeave;
        }
        protected override void UpdateUI(int elapsedMiliseconds)
        {
            int rowHeight = (int)(_menuLogoLines.Height / (float)MENU_LOGO_ROW_COUNT);
            float menuRemoveTweenerOffset = -300f * _menuRemoveTweener.Value;
            int lineFromMid = (int)(170f - menuRemoveTweenerOffset);
            bool menuFlashEnded = _milisecondsSinceStartup > _menuFlashEnd;
            int uiElementOffsetY = (int)(280f + menuRemoveTweenerOffset);
            int logoYoffset = (int)(512 - 30 - _menuLogoLines.Height + 8f * _menuLogoEaseTweener.Value * (1 - _menuRemoveTweener.Value));
            int logoYScreenCenter = (int)Centering.MiddleY(_menuLogoLines, 0, STOLON.V_HEIGHT).Y;
            logoYoffset -= (int)((logoYoffset - logoYScreenCenter) * _menuRemoveTweener.Value);
            const int MENU_LOGO_BOUNDS_CLEARING = 8;

            switch (_skipTo)
            {
                case "entity_select":
                    if (_milisecondsSinceStartup < 10000)
                    {
                        _milisecondsSinceStartup = 10001;
                        _menuDone = true;
                        _menuRemoveTweener.Update(10);
                        _boardPlayers = new Player[]
                                {
                                new Player("player0"),
                                new Player("player1"),
                                };
                        Leave();
                    }
                    break;
                case "main_menu":
                    if (_milisecondsSinceStartup < 10000) _milisecondsSinceStartup = 10001;
                    break;
            }

            #region inFlash
            _milisecondsSinceStartup += elapsedMiliseconds;
            _menuLogoDrawPos = Vector2.Round(Centering.MiddleX(_menuLogoLines, logoYoffset, STOLON.V_WIDTH));
            _menuLogoTileHider = new Rectangle(
                _menuLogoDrawPos.ToPoint() + new Point(0, (int)(rowHeight * (MENU_LOGO_ROW_COUNT - _menuLogoRowsHidden))),
                new Point((int)(_menuLogoLines.Width), (int)(rowHeight * _menuLogoRowsHidden))
            );

            _menuFlashStart = 1200;
            _menuFlashEnd = _menuFlashStart + 400;

            _menuLine1X = (int)(STOLON.V_WIDTH / 2f) - lineFromMid;
            _menuLine2X = (int)(STOLON.V_WIDTH / 2f) + lineFromMid;

            _menuLineLenght = _drawMenuLogoFilledTiles ? STOLON.V_HEIGHT : 0;
            _menuLineWidth = 2 + (menuFlashEnded ? 2 : 0);

            if (_milisecondsSinceStartup > 300) _menuLogoRowsHidden = 4;
            if (_milisecondsSinceStartup > 600) _menuLogoRowsHidden = 3;
            if (_milisecondsSinceStartup > 800) _menuLogoRowsHidden = 2;
            if (_milisecondsSinceStartup > 1000) _menuLogoRowsHidden = 1;
            if (_milisecondsSinceStartup > _menuFlashStart) _menuLogoRowsHidden = 0;

            if (_menuLogoFlashTime > 0)
            {
                _menuLogoMilisecondsFlashing += elapsedMiliseconds;
                if (_menuLogoMilisecondsFlashing > _menuLogoFlashTime)
                {
                    _drawMenuLogoFilledTiles = !_drawMenuLogoFilledTiles;
                    _menuLogoMilisecondsFlashing = 0;
                }
            }

            if (!_menuFlashStart.HasValue) return; // code below only relevant when the dummy board show animation ended.

            if (_milisecondsSinceStartup > _menuFlashStart.Value) _menuLogoFlashTime = 120;
            if (_milisecondsSinceStartup > _menuFlashStart.Value + 200) _menuLogoFlashTime = 100;
            if (_milisecondsSinceStartup > _menuFlashStart.Value + 300) _menuLogoFlashTime = 75;
            if (_milisecondsSinceStartup > _menuFlashStart.Value + 350) _menuLogoFlashTime = 60;
            if (_milisecondsSinceStartup < _menuFlashEnd) return; // code below only relevant when the full animation ended.

            #endregion
            #region inMenu
            _drawMenuLogoLowResFonted = true;
            _drawMenuLogoDummyTiles = true;
            _drawMenuLogoFilledTiles = true;
            _drawMenuLogoLines = true;

            _menuLogoFlashTime = 0; // ensures disabled flashing.
            _menuLogoEaseTweener.Update(elapsedMiliseconds / 1000f); // update the tweener.
            if (_menuLogoEaseTweener.Value == 1 || _menuLogoEaseTweener.Value == 0) // reverse and restart if finished.
            {
                _menuLogoEaseTweener.Reverse();
                _menuLogoEaseTweener.Start();
                STOLON.Debug.Log("reversed icon tweener.");
            }
            _menuLogoBoundingBox =
                new Rectangle(_menuLogoDrawPos.ToPoint() + new Point(-MENU_LOGO_BOUNDS_CLEARING), _menuLogoLines.Bounds.Size + new Point(MENU_LOGO_BOUNDS_CLEARING * 2));

            _menuDitherTexturePositions = new Point[(int)Math.Ceiling(STOLON.V_HEIGHT / (float)_dither32.Height) * 2];
            for (int i = 0; i < _menuDitherTexturePositions.Length; i++) // dithering positions.
                _menuDitherTexturePositions[i] = new Point(
                        (i >= _menuDitherTexturePositions.Length / 2f) ? _menuLine2X : _menuLine1X - _dither32.Width,
                        (i % (int)(_menuDitherTexturePositions.Length / 2f)) * _dither32.Height);

            UIOrdering.Order(STOLON.UI.UIElements.Values.ToArray(), STOLON.UI.MenuPath, STOLON.UI.DrawData, STOLON.UI.UIElementUpdateData, new Vector2(0, uiElementOffsetY), OrderProviders.Menu);

            if (STOLON.UI.UIElementUpdateData["xp_start"].IsClicked)
            {
                _boardPlayers = new Player[]
                        {
                            new Player("player0"),
                            new Player("player1"),
                        };
                Leave();
            }
            if (STOLON.UI.UIElementUpdateData["options"].IsClicked)
            {
                STOLON.UI.MenuPath = UIElement.GetSelfPath("options");
            }
            if (STOLON.UI.UIElementUpdateData["sound"].IsClicked)
            {
                STOLON.UI.MenuPath = UIElement.GetSelfPath("sound");
            }
            if (STOLON.UI.UIElementUpdateData["vol_up"].IsClicked)
            {
                STOLON.Audio.MasterVolume += 0.1001f;
                STOLON.Debug.Log("new volume: " + STOLON.Audio.MasterVolume);
            }
            if (STOLON.UI.UIElementUpdateData["vol_down"].IsClicked)
            {
                STOLON.Audio.MasterVolume -= 0.1001f;
                STOLON.Debug.Log("new volume: " + STOLON.Audio.MasterVolume);
            }
            if (STOLON.UI.UIElementUpdateData["story_start"].IsClicked)
            {
                STOLON.UI.Textframe.Queue(new DialogueInfo(STOLON.Environment, "Not yet implemented."));
            }
            if (STOLON.UI.UIElementUpdateData["com_start"].IsClicked)
            {
                _boardPlayers = new Player[]
                        {
                            new Player("player0"),
                            STOLON.Environment.Entities["goldsilk"].GetPlayer()
                        };
                Leave();
            }
            if (STOLON.UI.UIElementUpdateData["special_thanks"].IsClicked)
            {
                STOLON.UI.Textframe.Queue(new DialogueInfo(STOLON.Environment, "Please read the github README."));
            }
            if (STOLON.UI.UIElementUpdateData["quit"].IsClicked)
            {
                STOLON.Instance.Exit();
            }

            if (!_menuDone) return;
            #endregion

            _menuRemoveTweener.Update(elapsedMiliseconds / 1000f);
            TaskHeap.Instance.SafePush("menu_logo_disapear", new DynamicTask(() => // fire and forget game logic ftw
            {
                _onLeave?.Invoke();
                _onLeave = null;
                //STOLON.StateManager.ChangeState<BoardGameState>(true);
                //((BoardGameState)STOLON.StateManager.Current).SetBoard(_boardPlayers!);
                STOLON.StateManager.ChangeState<EntitySelectGameState>(true);
                _boardPlayers = null;
            }), 2000, false);
            _milisecondsSinceMenuRemoveStart += elapsedMiliseconds;

            _tipPos = Centering.MiddleX((int)(STOLON.Fonts[STOLON.SMALL_FONT_ID].FastMeasure(_tips[_tipId]).X),
                _menuLogoDrawPos.Y - STOLON.Fonts[STOLON.SMALL_FONT_ID].Dimensions.Y - (MENU_LOGO_BOUNDS_CLEARING * Math.Clamp(_menuRemoveTweener.Value * 2f, 0f, 1f)), STOLON.V_WIDTH, Vector2.One);

            _menuRemoveLineY = STOLON.V_HEIGHT - (int)(_menuRemoveTweener.Value * STOLON.V_HEIGHT);
            int lDelta = (int)(_menuLogoDrawPos.X - 8);
            MenuRemoveLine1x = lDelta;
            MenuRemoveLine2x = STOLON.V_WIDTH - lDelta;

            Centering.OnPixel(ref _menuLogoDrawPos);
        }
        public override void Draw(DrawingContext drawingContext, int elapsedMiliseconds)
        {
            drawingContext.DrawLine(_menuLine1X, -10f, _menuLine1X, _menuLineLenght, Color.White, _menuLineWidth);
            drawingContext.DrawLine(_menuLine2X, -10f, _menuLine2X, _menuLineLenght, Color.White, _menuLineWidth);
            if (_menuDone && _showSplashtexts) drawingContext.DrawString(STOLON.Fonts[STOLON.SMALL_FONT_ID], _tips[_tipId], _tipPos);

            if (_drawMenuLogoLowResFonted)
            {
                for (int i = 0; i < _menuDitherTexturePositions.Length; i++)
                    drawingContext.Draw(_dither32, _menuDitherTexturePositions[i].ToVector2(), effects: (i >= _menuDitherTexturePositions.Length / 2f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

                drawingContext.DrawArea(_menuLogoBoundingBox, Color.Black);
                drawingContext.DrawRectangle(_menuLogoBoundingBox, Color.White, UserInterface.LINE_WIDTH);

            }
            if (_drawMenuLogoDummyTiles) drawingContext.Draw(_menuLogoMarks, _menuLogoDrawPos);
            if (_drawMenuLogoFilledTiles) drawingContext.Draw(_menuLogoFilledMarks, _menuLogoDrawPos);
            if (_drawMenuLogoLowResFonted) drawingContext.Draw(_menuLogoFonted, _menuLogoDrawPos);

            drawingContext.DrawArea(_menuLogoTileHider, Color.Black);
            if (_drawMenuLogoLines) drawingContext.Draw(_menuLogoLines, _menuLogoDrawPos);

            drawingContext.DrawLine(MenuRemoveLine1x, STOLON.V_HEIGHT, MenuRemoveLine1x, _menuRemoveLineY, Color.White, UserInterface.LINE_WIDTH);
            drawingContext.DrawLine(MenuRemoveLine2x, STOLON.V_HEIGHT, MenuRemoveLine2x, _menuRemoveLineY, Color.White, UserInterface.LINE_WIDTH);

            if (_showEntityProfiles)
            {
                drawingContext.Draw(_entityProfiles["silo"], 512, new Vector2(STOLON.V_WIDTH / 2 + 64, 0), Vector2.One);
                drawingContext.Draw(_entityProfiles["deceit"], 512, new Vector2(STOLON.V_WIDTH / 2 - 130 - 512, -40), Vector2.One);
            }
        }
    }
}
