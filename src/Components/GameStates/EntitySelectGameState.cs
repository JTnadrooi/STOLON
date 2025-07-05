using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static STOLON.EntitySelectGameState;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    public class EntitySelectOrderProvider : IOrderProvider
    {
        private Font2D _font;
        private Vector2 _origin;

        private int _leftSpace;

        private const int PADDING_X = 8;
        private const int PADDING_Y = 4;

        public EntitySelectOrderProvider()
        {
            _font = STOLON.Fonts.Medium;
            _leftSpace = 0;
        }

        public void PrepareOrdering(Vector2 origin)
        {
            _origin = origin;
            _leftSpace = 0;
        }

        public UIElementDrawData GetDrawData(UIElement element, int index, out bool isHovered)
        {
            isHovered = false;
            Vector2 pos = _origin + new Vector2(_leftSpace, 0);
            Rectangle bounds = element.GetBounds(pos.ToPoint(), PADDING_X, PADDING_Y, 5, (int)(BOXED_TEXT_DIV_CLEARANCE / 2 - _font.Dimensions.Y / 2 - PADDING_Y), out Point textPos);
            _leftSpace += bounds.Width + 5;
            return new UIElementDrawData(element, element.Text.ToUpper(), _font, element.Type, textPos.ToVector2(), bounds, true, false, true);
        }
    }

    public class EntitySelectGameState : GameState
    {
        private readonly struct EntityDrawData
        {
            public Vector2 Pos { get; }
            public Rectangle SymbolNotationBox { get; }
            public EntityProfile Profile => Entity.Profile;
            public Entity Entity { get; }
            public bool Selected { get; }
            public string FullerName { get; }

            public EntityDrawData(Vector2 basePos, float floatAmount, Entity entity)
            {
                Pos = basePos + new Vector2(0, 10 * floatAmount);
                Entity = entity;
                SymbolNotationBox = new Rectangle(Pos.ToPoint(), new Point(28));
                FullerName = entity.FullName == entity.Name ? entity.Name : entity.Name + $" ({entity.FullName})";
            }

            public bool IsHovered() => new Rectangle(Pos.ToPoint(), new Point(128)).Contains(STOLON.Input.VirtualMousePos);
        }


        private Texture2D _tileTexture;
        private MenuGameState _menuGameState;

        private int _line1x;
        private int _line2x;
        private Tweener<float> _lineTweener;

        private int _entityCount;

        private bool _initDone;

        private EntityDrawData[] _drawData;

        private float[] _entityHoverCoefficients;
        private float _currentEntitySelectedCoefficient;
        private int _hoveredIndex;
        private TimedState<int> _hoveredState;

        private int _selectedIndex;
        private TimedState<int> _selectedState;

        private readonly Dictionary<int, Vector2> _posCache;
        private readonly Entity[] _entities;

        private OrderContainer<EntitySelectOrderProvider> _entityInfoContainer;
        private OrderContainer<EntitySelectOrderProvider> _lvlInfoContainer;

        //private Entity[] _entities;

        public const int TILE_SIZE = 128; // naming conventions for const variables aren't ALL_CAPS? oh no! anyway-
        public const int TILE_ROW_AMOUNT = 4;
        public const int TILE_COLUMN_AMOUNT = 2;
        public const int TILE_COUNT = TILE_ROW_AMOUNT * TILE_COLUMN_AMOUNT;

        public const int BOXED_TEXT_DIV_CLEARANCE = 32;
        public const int ROSTER_TOP_LINE = STOLON.V_HEIGHT - BOXED_TEXT_DIV_CLEARANCE;
        public const int ROSTER_BOTTOM_LINE = ROSTER_TOP_LINE - TILE_COLUMN_AMOUNT * TILE_SIZE;

        public const int LINE1_TARGET = TILE_SIZE * TILE_ROW_AMOUNT;
        public const int LINE2_TARGET = STOLON.V_WIDTH - BOXED_TEXT_DIV_CLEARANCE;

        private const float HOVER_INTENSITY = 0.25f;

        public const int INFO_WINDOW_TOPLINE = ROSTER_BOTTOM_LINE - BOXED_TEXT_DIV_CLEARANCE;

        public EntitySelectGameState() : base("entity_select")
        {
            _tileTexture = STOLON.Textures.GetReference("Debug\\temp-" + TILE_SIZE);
            if (!STOLON.StateManager.TryGetState(out _menuGameState!)) throw new Exception();
            _lineTweener = new Tweener<float>(0, 1, 2, Ease.Quad.InOut);
            _lineTweener.Start();
            _posCache = new Dictionary<int, Vector2>();

            _entities = STOLON.Environment.Entities.Values.ToArray();
            _entityCount = _entities.Length;
            _entityHoverCoefficients = new float[_entityCount];
            _hoveredIndex = -1;
            _selectedIndex = -1;
            _drawData = new EntityDrawData[_entityCount];

            _hoveredState = new TimedState<int>();
            _selectedState = new TimedState<int>();

            _entityInfoContainer = new OrderContainer<EntitySelectOrderProvider>(new EntitySelectOrderProvider(), [
                new UIElement("extended_name", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("synergy_warning", UIElement.TOP_ID, null, UIElementType.Ignore),
            ], new Vector2(0, INFO_WINDOW_TOPLINE));
            _lvlInfoContainer = new OrderContainer<EntitySelectOrderProvider>(new EntitySelectOrderProvider(), [
                new UIElement("lvl_name", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("lvl_diff", UIElement.TOP_ID, null, UIElementType.Ignore),
            ], new Vector2(0, STOLON.V_HEIGHT - BOXED_TEXT_DIV_CLEARANCE));
        }

        protected override void UpdateUI(int elapsedMilliseconds)
        {
            _lineTweener.Update(elapsedMilliseconds / 1000f);
            _initDone = !_lineTweener.Running;

            if (SkipAnimation && _lineTweener.Running) _lineTweener.Update(12f);

            _line1x = (int)MathHelper.Lerp(_menuGameState.RemoveLine1x, LINE1_TARGET, _lineTweener.Value);
            _line2x = (int)MathHelper.Lerp(_menuGameState.RemoveLine2x, LINE2_TARGET, _lineTweener.Value);

            if (_lineTweener.Running) return;

            _hoveredState.UpdatePositive(_hoveredIndex, elapsedMilliseconds);
            _selectedState.UpdatePositive(_selectedIndex, elapsedMilliseconds);

            _hoveredIndex = -1;
            for (int i = 0; i < _entityCount; i++)
            {
                Vector2 basePos = GetBaseTilePos(i);
                if (new Rectangle(basePos.ToPoint(), new Point(128)).Contains(STOLON.Input.VirtualMousePos))
                {
                    _hoveredIndex = i;
                    _entityHoverCoefficients[i] = MathHelper.Lerp(_entityHoverCoefficients[i], 1, HOVER_INTENSITY);
                    if (STOLON.Input.IsClicked(GameInput.MouseButton.Left))
                        if (_selectedIndex == i) _selectedIndex = -1;
                        else
                        {
                            _selectedIndex = i;
                            _currentEntitySelectedCoefficient = 0;
                            STOLON.Debug.Log("changed selected to " + i + ".");
                        }
                }
                else _entityHoverCoefficients[i] = MathHelper.Lerp(_entityHoverCoefficients[i], 0, HOVER_INTENSITY / 5);

                _drawData[i] = new EntityDrawData(basePos, _entityHoverCoefficients[i], _entities[i]);
            }

            if (_selectedIndex == -1)
            {
                _currentEntitySelectedCoefficient = 0;
            }
            else
            {
                _currentEntitySelectedCoefficient = MathHelper.Lerp(_currentEntitySelectedCoefficient, 1, HOVER_INTENSITY / 2);

                _entityInfoContainer.Position = new Vector2((_currentEntitySelectedCoefficient - 1) * 200, INFO_WINDOW_TOPLINE);

                _entityInfoContainer.Elements["extended_name"].Text = _drawData[_selectedIndex].FullerName;
                _entityInfoContainer.Elements["synergy_warning"].Text = "72%";
                _entityInfoContainer.Update(elapsedMilliseconds);
            }

            _lvlInfoContainer.Elements["lvl_name"].Text = "Node 12b: LANU LANU LANU";
            _lvlInfoContainer.Elements["lvl_diff"].Text = "Difficulty 5";
            _lvlInfoContainer.Update(elapsedMilliseconds);
        }

        public Vector2 GetBaseTilePos(int i)
            => _posCache.TryGetValue(i, out Vector2 cachedPos) ? cachedPos :
                _posCache[i] = new Vector2((i % TILE_ROW_AMOUNT) * TILE_SIZE, (TILE_COLUMN_AMOUNT - 1 - i / TILE_ROW_AMOUNT) * TILE_SIZE + (STOLON.V_HEIGHT - BOXED_TEXT_DIV_CLEARANCE - TILE_SIZE * TILE_COLUMN_AMOUNT));

        public override void Draw(DrawingContext drawingContext)
        {
            if (_initDone)
            {
                drawingContext.DrawArea(new Rectangle(0, 0, _line1x, 1000), Color.Black);

                drawingContext.DrawLine(0, INFO_WINDOW_TOPLINE, TILE_SIZE * TILE_ROW_AMOUNT, INFO_WINDOW_TOPLINE, Color.White, UserInterface.LINE_WIDTH);
                if (_selectedIndex != -1)
                {
                    Entity selectedEntity = _drawData[_selectedIndex].Entity;
                    _entityInfoContainer.Draw(drawingContext);
                }

                drawingContext.DrawLine(0, ROSTER_TOP_LINE, TILE_SIZE * TILE_ROW_AMOUNT, ROSTER_TOP_LINE, Color.White, UserInterface.LINE_WIDTH);
                drawingContext.DrawLine(0, ROSTER_BOTTOM_LINE, TILE_SIZE * TILE_ROW_AMOUNT, ROSTER_BOTTOM_LINE, Color.White, UserInterface.LINE_WIDTH);
                for (int i = 0; i < TILE_COUNT; i++)
                    if (i < _entityCount)
                    {
                        ref EntityDrawData ddc = ref _drawData[i];

                        drawingContext.DrawEntity(ddc.Profile, TILE_SIZE, ddc.Pos, drawMode: EntityDrawMode.WithBackground);

                        if (_hoveredIndex == i) drawingContext.Draw(STOLON.Textures["UI\\spotlight-128"], ddc.Pos);

                        drawingContext.DrawSymbolNotation(ddc.Entity.SymbolNotation, ddc.SymbolNotationBox);
                        drawingContext.DrawRectangle(new Rectangle(ddc.Pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);

                        if (_selectedIndex == i) drawingContext.Draw(STOLON.Textures["UI\\profile_selected"], ddc.Pos + new Vector2(0, -32));
                    }
                    else
                    {
                        Vector2 pos = GetBaseTilePos(i);
                        drawingContext.Draw(STOLON.Textures["UI\\profile_question-128"], pos);
                        drawingContext.DrawRectangle(new Rectangle(pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);
                    }
                _lvlInfoContainer.Draw(drawingContext);

                //drawingContext.DrawString(STOLON.Fonts.Medium, "ENTITY #" + typeof(GoldsilkEntity).GetHashCode(), new Vector2(STOLON.V_WIDTH - 4f, 10f), rotation: 1.57079633f);
            }
            drawingContext.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, UserInterface.LINE_WIDTH);
            drawingContext.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
