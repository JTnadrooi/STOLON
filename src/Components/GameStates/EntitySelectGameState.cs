using AsitLib;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static STOLON.EntitySelectGameState;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    public class EntitySelectOrderProvider : IOrderProvider
    {
        private Font2D _font;
        private Vector2 _origin;

        private int _leftSpace;

        private float[]? _elementHovData;

        private const int PADDING_X = 8;
        private const int PADDING_Y = 4;

        private const float HOVER_INTENSITY = 0.1f;

        public EntitySelectOrderProvider()
        {
            _font = STOLON.Fonts.Medium;
            _leftSpace = 0;
        }

        public void PrepareOrdering(Vector2 origin, int elementCount)
        {
            _origin = origin;
            _leftSpace = 0;
            if (_elementHovData == null || _elementHovData.Length != elementCount) _elementHovData = new float[elementCount];
        }
        public UIElementDrawData GetDrawData(UIElement element, int index, out bool isHovered)
        {
            Vector2 pos = _origin + new Vector2(_leftSpace, 0);

            isHovered = element.GetBounds(pos.ToPoint(), PADDING_X, PADDING_Y, 5, 0, out _).Contains(STOLON.Input.VirtualMousePos);

            if (isHovered)  _elementHovData[index] = MathHelper.Lerp(_elementHovData[index], 1, HOVER_INTENSITY * 2);
            else  _elementHovData[index] = MathHelper.Lerp(_elementHovData[index], 0, HOVER_INTENSITY);

            float hoverHeightBoost = 4f * _elementHovData[index]; 
            int hoverPaddingY = PADDING_Y + (int)hoverHeightBoost;

            Rectangle bounds = element.GetBounds(pos.ToPoint(), PADDING_X, hoverPaddingY, 5, (int)(BOXED_TEXT_DIV_CLEARANCE / 2 - _font.Dimensions.Y / 2 - hoverPaddingY), out Point textPos);
            _leftSpace += bounds.Width + 5;
            return new UIElementDrawData(element, element.Text.ToUpper(), _font, element.Type, textPos.ToVector2(), bounds, true, false, true);
        }
    }

    public class EntitySelectGameState : GameState
    {
        #region SUBSTRUCTS

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

        private readonly struct EntityAllocationData
        {
            public int VirtualAllocation { get; }
            public int Allocation { get; }
            public Entity Entity { get; }

            public EntityAllocationData(int allocation, Entity entity)
            {
                Allocation = allocation;
                VirtualAllocation = (int)(allocation * 1.2f);
                Entity = entity;
            }
        }

        #endregion

        private Texture2D _tileTexture;
        private MenuGameState _menuGameState;

        private int _line1x;
        private int _line2x;
        private Tweener<float> _lineTweener;

        private int _entityCount;

        private bool _initDone;

        private EntityDrawData[] _entityDrawDump;
        private EntityAllocationData?[] _allocationDataDump;
        private int[] _selection;

        private float[] _entityHoverCoefficients;
        private float _currentEntitySelectedCoefficient;
        private int _hoveredIndex;
        private TimedState<int> _hoveredState;

        private int _lastSelected;
        private TimedState<int> _selectedState;

        private readonly Dictionary<int, Vector2> _posCache;
        private readonly Entity[] _entities;

        private OrderContainer<EntitySelectOrderProvider> _entityInfoContainer;
        private OrderContainer<EntitySelectOrderProvider> _lvlInfoContainer;

        #region CONSTANTS

        public const int TILE_SIZE = 128; // naming conventions for const variables aren't ALL_CAPS? oh no! anyway-
        public const int TILE_ROW_AMOUNT = 5;
        public const int TILE_COLUMN_AMOUNT = 2;
        public const int TILE_COUNT = TILE_ROW_AMOUNT * TILE_COLUMN_AMOUNT;

        public const int BOXED_TEXT_DIV_CLEARANCE = 32;
        public const int ROSTER_TOP_LINE = STOLON.V_HEIGHT - BOXED_TEXT_DIV_CLEARANCE;
        public const int ROSTER_BOTTOM_LINE = ROSTER_TOP_LINE - TILE_COLUMN_AMOUNT * TILE_SIZE;

        public const int LINE1_TARGET = TILE_SIZE * TILE_ROW_AMOUNT;
        public const int LINE2_TARGET = STOLON.V_WIDTH - BOXED_TEXT_DIV_CLEARANCE;

        private const float HOVER_INTENSITY = 0.25f;

        public const int INFO_WINDOW_TOPLINE = ROSTER_BOTTOM_LINE - BOXED_TEXT_DIV_CLEARANCE;

        public const int MAX_SELECTION = 4;

        #endregion

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
            _lastSelected = -1;
            _entityDrawDump = new EntityDrawData[_entityCount];
            _allocationDataDump = new EntityAllocationData?[MAX_SELECTION];

            _hoveredState = new TimedState<int>();
            _selectedState = new TimedState<int>();
            _selection = Enumerable.Repeat(-1, MAX_SELECTION).ToArray();

            _entityInfoContainer = new OrderContainer<EntitySelectOrderProvider>(new EntitySelectOrderProvider(), [
                new UIElement("extended_name", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("alloc", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("v_alloc", UIElement.TOP_ID, null, UIElementType.Ignore),
            ], new Vector2(0, INFO_WINDOW_TOPLINE));
            _lvlInfoContainer = new OrderContainer<EntitySelectOrderProvider>(new EntitySelectOrderProvider(), [
                new UIElement("lvl_name", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("lvl_diff", UIElement.TOP_ID, null, UIElementType.Ignore),
            ], new Vector2(0, STOLON.V_HEIGHT - BOXED_TEXT_DIV_CLEARANCE));

            if (SkipArgs != null)
            {
                _lastSelected = SkipArgs[0] != "-1" ? _entities.GetFirstIndexWhere(e => e.Id == SkipArgs[0]) : _lastSelected;
            }

            STOLON.UI.Textframe.Hide = true;
        }
        protected override void UpdateUI(int elapsedMilliseconds)
        {
            float deltaTime = elapsedMilliseconds / 1000f;

            _lineTweener.Update(deltaTime);
            _initDone = !_lineTweener.Running;

            if (SkipAnimation && _lineTweener.Running)
                _lineTweener.Update(12f); // skip animation instantly.

            _line1x = (int)MathHelper.Lerp(_menuGameState.RemoveLine1x, LINE1_TARGET, _lineTweener.Value);
            _line2x = (int)MathHelper.Lerp(_menuGameState.RemoveLine2x, LINE2_TARGET, _lineTweener.Value);

            if (_lineTweener.Running)
                return;

            // update UI animation states.
            _hoveredState.UpdatePositive(_hoveredIndex, elapsedMilliseconds);
            _selectedState.UpdatePositive(_lastSelected, elapsedMilliseconds);

            _hoveredIndex = -1;

            for (int i = 0; i < _entityCount; i++)
            {
                Vector2 basePos = GetBaseTilePos(i);
                Rectangle entityRect = new Rectangle(basePos.ToPoint(), new Point(128));

                bool isHovered = entityRect.Contains(STOLON.Input.VirtualMousePos);
                _entityHoverCoefficients[i] = MathHelper.Lerp(_entityHoverCoefficients[i], isHovered ? 1 : 0, isHovered ? HOVER_INTENSITY : HOVER_INTENSITY / 5);

                if (isHovered)
                {
                    _hoveredIndex = i;

                    Point clickZoneOffset = new Point(0, TILE_SIZE - 32);
                    Rectangle clickZone = new Rectangle(basePos.ToPoint() + clickZoneOffset, new Point(32));

                    if (clickZone.Contains(STOLON.Input.VirtualMousePos) && STOLON.Input.IsClicked(GameInput.MouseButton.Left))
                    {
                        if (_selection.Contains(i)) RemoveFromSelection(i);
                        else AddToSelection(i);
                    }
                    else if (STOLON.Input.IsClicked(GameInput.MouseButton.Left))
                    {
                        if (_lastSelected == i)
                        {
                            _lastSelected = -1; // entity deselection.
                        }
                        else
                        {
                            _lastSelected = i; // new entity selection.
                            _currentEntitySelectedCoefficient = 0;
                            STOLON.Debug.Log("changed selected entity to " + i + ".");
                        }
                    }
                }

                _entityDrawDump[i] = new EntityDrawData(basePos, _entityHoverCoefficients[i], _entities[i]);
            }

            if (_lastSelected == -1)
            {
                _currentEntitySelectedCoefficient = 0; // no entity selected.
            }
            else
            {
                // update selected entity UI panel.
                _currentEntitySelectedCoefficient = MathHelper.Lerp(_currentEntitySelectedCoefficient, 1, HOVER_INTENSITY / 2);
                _entityInfoContainer.Position = new Vector2((_currentEntitySelectedCoefficient - 1) * 200, INFO_WINDOW_TOPLINE);

                _entityInfoContainer.Elements["extended_name"].Text = _entityDrawDump[_lastSelected].FullerName;
                _entityInfoContainer.Elements["alloc"].Text = "(alloc) 100%";
                _entityInfoContainer.Elements["v_alloc"].Text = "(valloc) 100%";

                _entityInfoContainer.Update(elapsedMilliseconds);
            }

            // update level info container.
            _lvlInfoContainer.Elements["lvl_name"].Text = "STOLON Test Level";
            _lvlInfoContainer.Elements["lvl_diff"].Text = "Difficulty 1";
            _lvlInfoContainer.Update(elapsedMilliseconds);
        }


        public void AddToSelection(int entityIndex)
        {
            STOLON.Debug.Log("selecting entity " + entityIndex + ".");
            for (int i2 = 0; i2 < _selection.Length; i2++)
                if (_selection[i2] == -1)
                {
                    _selection[i2] = entityIndex;
                    break;
                }
            UpdateAllocations();
        }
        public void RemoveFromSelection(int entityIndex)
        {
            STOLON.Debug.Log("deselecting entity " + entityIndex + ".");
            _selection[_selection.GetFirstIndexWhere(i => entityIndex == i)] = -1;
            UpdateAllocations();
        }
        private void UpdateAllocations()
        {
            STOLON.Debug.Log("updating allocations..");

            HashSet<Entity> selectedEntities = _selection.WhereSelect(id => (id != -1 ? _entities[id] : null!, id != -1)).ToHashSet();
            for (int i = 0; i < _selection.Length; i++)
            {
                if (_selection[i] == -1) _allocationDataDump[i] = null;

                _allocationDataDump[i] = new EntityAllocationData(100, _entities[i]);
            }
            STOLON.Debug.Success();
        }

        public Vector2 GetBaseTilePos(int i)
            => _posCache.TryGetValue(i, out Vector2 cachedPos) ? cachedPos :
                _posCache[i] = new Vector2((i % TILE_ROW_AMOUNT) * TILE_SIZE, (TILE_COLUMN_AMOUNT - 1 - i / TILE_ROW_AMOUNT) * TILE_SIZE + (STOLON.V_HEIGHT - BOXED_TEXT_DIV_CLEARANCE - TILE_SIZE * TILE_COLUMN_AMOUNT));

        public override void Draw(DrawingContext drawingContext)
        {
            if (_initDone)
            {
                // draw selected entity preview.
                if (_lastSelected != -1)
                {
                    Entity selectedEntity = _entityDrawDump[_lastSelected].Entity;
                    drawingContext.DrawEntity(selectedEntity, 512, new Vector2(STOLON.V_WIDTH - 415, 0));
                }

                // draw side black areas.
                drawingContext.DrawArea(new Rectangle(0, 0, _line1x, 1000), Color.Black);
                drawingContext.DrawArea(new Rectangle(_line2x, 0, STOLON.V_WIDTH - _line2x, 1000), Color.Black);

                // draw vertical and horizontal layout lines.
                drawingContext.DrawLine(0, INFO_WINDOW_TOPLINE, TILE_SIZE * TILE_ROW_AMOUNT, INFO_WINDOW_TOPLINE, Color.White, Interface.LINE_WIDTH);
                drawingContext.DrawLine(TILE_SIZE * 2, INFO_WINDOW_TOPLINE, TILE_SIZE * 2, 0, Color.White, Interface.LINE_WIDTH);
                drawingContext.DrawLine(TILE_SIZE * 3, INFO_WINDOW_TOPLINE, TILE_SIZE * 3, 0, Color.White, Interface.LINE_WIDTH);

                // draw entity info window.
                if (_lastSelected != -1)
                {
                    Entity selectedEntity = _entityDrawDump[_lastSelected].Entity;
                    drawingContext.Draw(_entityInfoContainer);

                    string description = selectedEntity.Description ?? string.Empty;
                    string wrapped = STOLON.Fonts.Small.Wrap(description, TILE_SIZE * 2 - 10, INFO_WINDOW_TOPLINE - 12, 0, out int lc).ToUpper();
                    Vector2 textPos = new Vector2(10, INFO_WINDOW_TOPLINE - lc * STOLON.Fonts.Small.Dimensions.Y - 10);
                    drawingContext.DrawString(STOLON.Fonts.Small, wrapped, textPos);
                }

                // draw roster lines.
                drawingContext.DrawLine(0, ROSTER_TOP_LINE, TILE_SIZE * TILE_ROW_AMOUNT, ROSTER_TOP_LINE, Color.White, Interface.LINE_WIDTH);
                drawingContext.DrawLine(0, ROSTER_BOTTOM_LINE, TILE_SIZE * TILE_ROW_AMOUNT, ROSTER_BOTTOM_LINE, Color.White, Interface.LINE_WIDTH);

                // draw entity tiles.
                for (int i = 0; i < TILE_COUNT; i++)
                {
                    if (i < _entityCount)
                    {
                        ref EntityDrawData ddc = ref _entityDrawDump[i];

                        drawingContext.DrawEntity(ddc.Profile, TILE_SIZE, ddc.Pos, drawMode: EntityDrawMode.WithBackground);

                        bool isHovered = (_hoveredIndex == i);
                        bool isSelected = _selection.Contains(i);

                        if (isHovered)
                        {
                            drawingContext.Draw(STOLON.Textures["UI\\spotlight-128"], ddc.Pos);
                            if (!isSelected) drawingContext.Draw(STOLON.Textures["UI\\selected_add-overlay"], ddc.Pos);
                        }

                        if (isSelected)
                        {
                            int selectedIndex = _selection.GetFirstIndexWhere(x => i == x);
                            drawingContext.Draw(STOLON.Textures[$"UI\\selected_{selectedIndex + 1}-overlay"], ddc.Pos);
                            if (isHovered) drawingContext.Draw(STOLON.Textures["UI\\selected_remove-overlay"], ddc.Pos);
                        }

                        drawingContext.DrawSymbolNotation(ddc.Entity.SymbolNotation, ddc.SymbolNotationBox);
                        drawingContext.DrawRectangle(new Rectangle(ddc.Pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);

                        if (_lastSelected == i)
                            drawingContext.Draw(STOLON.Textures["UI\\profile_selected"], ddc.Pos + new Vector2(0, -32));
                    }
                    else
                    {
                        Vector2 pos = GetBaseTilePos(i);
                        drawingContext.Draw(STOLON.Textures["UI\\profile_question-128"], pos);
                        drawingContext.DrawRectangle(new Rectangle(pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);
                    }
                }

                // draw level info.
                drawingContext.Draw(_lvlInfoContainer);
            }

            drawingContext.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, Interface.LINE_WIDTH);
            drawingContext.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, Interface.LINE_WIDTH);
        }

    }
}
