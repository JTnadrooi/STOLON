using AsitLib;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static STOLON.EntitySelectGameState;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    public struct SelectionEntry
    {
        public Entity Entity { get; }
        public int Allocation { get; }
        public SelectionEntry(Entity entity, int alloc)
        {
            Entity = entity;
            Allocation = alloc;
        }
    }
    public struct SelectionInfo
    {
        public ReadOnlyDictionary<string, SelectionEntry> Entries { get; }

        public SelectionInfo(SelectionEntry[] entries)
        {
            Entries = entries.ToDictionary(e => e.Entity.Id).AsReadOnly();
        }

        //public bool IsSelected<TEntity>() where TEntity : Entity => IsSelected(STOLON.Environment.GetEntityInstance<TEntity>().Id);
        public bool IsSelected(string id) => Entries.ContainsKey(id);
        public int GetAllocation(string id) => Entries[id].Allocation;

        public static SelectionInfo Empty { get; } = new SelectionInfo(Array.Empty<SelectionEntry>());
    }
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

        public void PrepareOrdering(Vector2 origin, int elementCount)
        {
            _origin = origin;
            _leftSpace = 0;
        }

        public UIElementDrawData GetDrawData(UIElement element, int index, out bool isHovered)
        {
            Vector2 pos = _origin + new Vector2(_leftSpace, 0);
            isHovered = element.GetBounds(pos.ToPoint(), PADDING_X, PADDING_Y, 5, 0, out _).Contains(STOLON.Input.VirtualMousePos);
            Rectangle bounds = element.GetBounds(pos.ToPoint(), PADDING_X, PADDING_Y, 5, (int)(BOXED_TEXT_DIV_CLEARANCE / 2 - _font.Dimensions.Y / 2 - PADDING_Y), out Point textPos);
            _leftSpace += bounds.Width + 5;
            return new UIElementDrawData(element, element.Text.ToUpper(), _font, element.Type, textPos.ToVector2(), bounds, true, false, true);
        }
    }

    public class ConditionalNoteEnumerationGraphic : IGraphic
    {
        public ConditionalNote[] Notes { get; set; }
        public Vector2 Pos { get; }
        public int TextWidth { get; }

        private EntitySelectGameState _entitySelect;
        private CachedNoteData[] _cachedNotes;

        private readonly record struct CachedNoteData(string WrappedText, int LineCount, bool IsActive);

        private const int NOTE_CLEARANCE = 12;
        private const int NOTE_BORDER_X_CLEARANCE = 10;

        public ConditionalNoteEnumerationGraphic(EntitySelectGameState entitySelect, Vector2 pos, int textWidth)
        {
            Notes = Array.Empty<ConditionalNote>();
            Pos = pos;
            TextWidth = textWidth;
            _cachedNotes = Array.Empty<CachedNoteData>();
            _entitySelect = entitySelect;
        }

        public void Update(int elapsedMilliseconds)
        {
            if (Notes.Length != _cachedNotes.Length) _cachedNotes = new CachedNoteData[Notes.Length];
            for (int i = 0; i < Notes.Length; i++)
                _cachedNotes[i] = new CachedNoteData(STOLON.Fonts.Small.Wrap(Notes[i].Text, TextWidth - NOTE_BORDER_X_CLEARANCE * 2 - NOTE_CLEARANCE, int.MaxValue, out var lc).ToUpper(), lc, Notes[i].IsActive(_entitySelect.Selection));
        }

        public void Draw(DrawingContext drawingContext)
        {
            int notesClearingUp = 7, noteSpacing = STOLON.Fonts.Small.CoreFont.LineHeight / 2;

            for (int i = 0; i < _cachedNotes.Length; i++)
            {
                CachedNoteData note = _cachedNotes[i];
                drawingContext.DrawString(STOLON.Fonts.Small, note.WrappedText, new((int)Pos.X + NOTE_BORDER_X_CLEARANCE + NOTE_CLEARANCE, (int)Pos.Y - notesClearingUp - note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight));
                drawingContext.DrawString(STOLON.Fonts.Small, "-", new((int)Pos.X + NOTE_BORDER_X_CLEARANCE, (int)Pos.Y - notesClearingUp - STOLON.Fonts.Small.CoreFont.LineHeight));

                if (note.IsActive)
                    drawingContext.DrawRectangle(new((int)Pos.X + 4, (int)Pos.Y - notesClearingUp - note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight - 3, TextWidth - 8, note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight + 6), thickness: 1);

                notesClearingUp += note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight + noteSpacing;
            }
        }
    }

    public class EntitySelectGameState : GameState
    {
        #region SUBSTRUCTS

        private readonly struct EntityDrawData
        {
            public readonly Vector2 Pos;
            public readonly Rectangle SymbolNotationBox;
            public readonly Entity Entity;
            public readonly bool Selected;
            public readonly string FullerName;

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
            public readonly int VirtualAllocation;
            public readonly int Allocation;
            public readonly Entity Entity;

            public EntityAllocationData(int allocation, int virtualAllocation, Entity entity)
            {
                Allocation = allocation;
                VirtualAllocation = virtualAllocation;
                Entity = entity;
            }
        }

        private readonly struct EntityDrawAllocationData
        {
            public readonly Rectangle SymbolNotationRect;
            public readonly Rectangle AllocationRect;
            public readonly Rectangle VirtualAllocationRect;

            public EntityDrawAllocationData(int x)
            {
                Rectangle GetMiniSlot(int h) => new Rectangle(x, INFO_WINDOW_TOPLINE - 32 - SYMBOL_NOTATION_SIZE * h, SYMBOL_NOTATION_SIZE, SYMBOL_NOTATION_SIZE);

                SymbolNotationRect = GetMiniSlot(0);
                AllocationRect = GetMiniSlot(1);
                VirtualAllocationRect = GetMiniSlot(2);
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
        private EntityDrawAllocationData?[] _drawAllocationDataDump;

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

        private ConditionalNoteEnumerationGraphic _allocNotes;

        private BoardState _boardState;
        private BoardPreview _boardPreview;
        private bool _drawConnectionLine;
        private Line _connectionLine;

        public SelectionInfo Selection { get; private set; }
        private Entity SelectedEntity => _entityDrawDump[_lastSelected].Entity;

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

        public const int ADD_BOX_SIZE = 32;

        public const int SYMBOL_NOTATION_SIZE = TILE_SIZE / MAX_SELECTION;

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
            _lastSelected = new Random().Next(0, _entityCount);
            _entityDrawDump = new EntityDrawData[_entityCount];
            _allocationDataDump = new EntityAllocationData?[MAX_SELECTION];
            _drawAllocationDataDump = new EntityDrawAllocationData?[MAX_SELECTION];

            _hoveredState = new TimedState<int>();
            _selectedState = new TimedState<int>();
            _selection = Enumerable.Repeat(-1, MAX_SELECTION).ToArray();
            _boardState = BoardState.GetDefault([new Player("player0"), STOLON.Environment.Entities["goldsilk"].GetPlayer()]);
            _boardPreview = _boardState.GetPreview();

            _entityInfoContainer = new OrderContainer<EntitySelectOrderProvider>(new EntitySelectOrderProvider(), [
                new UIElement("extended_name", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("alloc", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("v_alloc", UIElement.TOP_ID, null, UIElementType.Ignore),
            ], new Vector2(0, INFO_WINDOW_TOPLINE));
            _lvlInfoContainer = new OrderContainer<EntitySelectOrderProvider>(new EntitySelectOrderProvider(), [
                new UIElement("lvl_name", UIElement.TOP_ID, null, UIElementType.Ignore),
                new UIElement("lvl_diff", UIElement.TOP_ID, null, UIElementType.Ignore),
            ], new Vector2(0, STOLON.V_HEIGHT - BOXED_TEXT_DIV_CLEARANCE));

            _allocNotes = new ConditionalNoteEnumerationGraphic(this, new Vector2(0, INFO_WINDOW_TOPLINE), TILE_SIZE);

            if (SkipArgs != null)
            {
                _lastSelected = SkipArgs[0] != "-1" ? _entities.GetFirstIndexWhere(e => e.Id == SkipArgs[0]) : _lastSelected;
            }

            Selection = SelectionInfo.Empty;

            STOLON.UI.Textframe.Hide = true;
        }
        protected override void UpdateUI(int elapsedMilliseconds)
        {
            float deltaTime = elapsedMilliseconds / 1000f;
            _drawConnectionLine = false;

            _lineTweener.Update(deltaTime);
            _initDone = !_lineTweener.Running;

            if (SkipAnimation && _lineTweener.Running)
                _lineTweener.Update(12f); // skip animation instantly.

            _line1x = (int)MathHelper.Lerp(_menuGameState.RemoveLine1x, LINE1_TARGET, _lineTweener.Value);
            _line2x = (int)MathHelper.Lerp(_menuGameState.RemoveLine2x, LINE2_TARGET, _lineTweener.Value);

            if (_lineTweener.Running) return;

            // update UI animation states.
            _hoveredState.UpdatePositive(_hoveredIndex, elapsedMilliseconds);
            _selectedState.UpdatePositive(_lastSelected, elapsedMilliseconds);

            _hoveredIndex = -1;

            #region TILES

            for (int entityIndex = 0; entityIndex < _entityCount; entityIndex++)
            {
                Vector2 basePos = GetBaseTilePos(entityIndex);
                Rectangle entityRect = new Rectangle(basePos.ToPoint(), new Point(128));

                bool isHovered = entityRect.Contains(STOLON.Input.VirtualMousePos);
                _entityHoverCoefficients[entityIndex] = MathHelper.Lerp(_entityHoverCoefficients[entityIndex], isHovered ? 1 : 0, isHovered ? HOVER_INTENSITY : HOVER_INTENSITY / 5);

                if (isHovered)
                {
                    _hoveredIndex = entityIndex;

                    Rectangle addBox = new Rectangle(basePos.ToPoint() + new Point(0, TILE_SIZE - ADD_BOX_SIZE), new Point(32));

                    if (addBox.Contains(STOLON.Input.VirtualMousePos) && STOLON.Input.IsClicked(GameInput.MouseButton.Left))
                        if (_selection.Contains(entityIndex)) RemoveFromSelection(entityIndex);
                        else AddToSelection(entityIndex);
                    else if (STOLON.Input.IsClicked(GameInput.MouseButton.Left))
                    {
                        if (_lastSelected != entityIndex)
                        {
                            _lastSelected = entityIndex; // new entity selection.
                            _currentEntitySelectedCoefficient = 0;
                            STOLON.Debug.Log("changed selected entity to " + entityIndex + ".");
                        }
                    }
                }

                _entityDrawDump[entityIndex] = new EntityDrawData(basePos, _entityHoverCoefficients[entityIndex], _entities[entityIndex]);
            }

            for (int slotIndex = 0; slotIndex < _selection.Length; slotIndex++)
            {
                int entityIndex = _selection[slotIndex];
                if (entityIndex == -1) continue;
                if (_hoveredIndex == entityIndex || _drawAllocationDataDump[slotIndex]!.Value.SymbolNotationRect.Contains(STOLON.Input.VirtualMousePos))
                {
                    _drawConnectionLine = true;
                    _connectionLine = new Line(_entityDrawDump[entityIndex].Pos.ToPoint() + new Point(TILE_SIZE / 2, 0), _drawAllocationDataDump[GetSlot(entityIndex)]!.Value.SymbolNotationRect.Location + new Point(SYMBOL_NOTATION_SIZE / 2, SYMBOL_NOTATION_SIZE));
                }
            }

            #endregion

            const int BOARDPREVIEW_CLEARANCE = 0;
            _boardPreview.Pos = new Vector2(LINE1_TARGET - BOARDPREVIEW_CLEARANCE - _boardPreview.Dimensions.X, INFO_WINDOW_TOPLINE - BOARDPREVIEW_CLEARANCE - _boardPreview.Dimensions.Y);

            // update selected entity UI panel.
            _currentEntitySelectedCoefficient = MathHelper.Lerp(_currentEntitySelectedCoefficient, 1, HOVER_INTENSITY / 2);
            _entityInfoContainer.Position = new Vector2((_currentEntitySelectedCoefficient - 1) * 200, INFO_WINDOW_TOPLINE);

            _entityInfoContainer.Elements["extended_name"].Text = _entityDrawDump[_lastSelected].FullerName;
            _entityInfoContainer.Elements["alloc"].Text = $"(alloc) {(IsInSelection(_lastSelected) ? _allocationDataDump[GetSlot(_lastSelected)]!.Value.Allocation : 0)}%";
            _entityInfoContainer.Elements["v_alloc"].Text = $"(valloc) {(IsInSelection(_lastSelected) ? _allocationDataDump[GetSlot(_lastSelected)]!.Value.VirtualAllocation : 0)}%";
            _entityInfoContainer.Update(elapsedMilliseconds);

            // update level info container.
            _lvlInfoContainer.Elements["lvl_name"].Text = "STOLON Test Level";
            _lvlInfoContainer.Elements["lvl_diff"].Text = "Difficulty 1";
            _lvlInfoContainer.Update(elapsedMilliseconds);

            _allocNotes.Notes = SelectedEntity.Notes;
            _allocNotes.Update(elapsedMilliseconds);
        }

        public void AddToSelection(int entityIndex)
        {
            STOLON.Debug.Log(">selecting entity " + entityIndex + ".");
            for (int slotIndex = 0; slotIndex < MAX_SELECTION; slotIndex++)
                if (_selection[slotIndex] == -1)
                {
                    _selection[slotIndex] = entityIndex;
                    break;
                }
            UpdateSelection();
            STOLON.Debug.Success();
        }
        public int GetSlot(int entityIndex) => _selection.GetFirstIndexWhere(s => s == entityIndex);
        public bool IsInSelection(int entityIndex) => _selection.Any(x => x == entityIndex);
        public bool IsSlotOccupied(int slotIndex) => _selection[slotIndex] != -1;
        public void RemoveFromSelection(int entityIndex)
        {
            STOLON.Debug.Log(">deselecting entity " + entityIndex + ".");
            _selection[_selection.GetFirstIndexWhere(i => entityIndex == i)] = -1;
            UpdateSelection();
            STOLON.Debug.Success();
        }
        private void UpdateSelection()
        {
            STOLON.Debug.Log(">updating allocations (and draw data)..");

            HashSet<Entity> selectedEntities = _selection.WhereSelect(id => (id != -1 ? _entities[id] : null!, id != -1)).ToHashSet();
            int usedSlots = _selection.Where(i => i != -1).Count();
            int secondarySymbolPosOffsetX = TILE_SIZE * 2;

            Selection = new SelectionInfo(_selection.Where(i => i != -1).Select(i => new SelectionEntry(_entities[i], 100 / usedSlots)).ToArray());
            for (int slotIndex = 0; slotIndex < MAX_SELECTION; slotIndex++)
            {
                STOLON.Debug.Log($">checking slot {slotIndex}..");
                if (_selection[slotIndex] == -1)
                {
                    _allocationDataDump[slotIndex] = null;
                    _drawAllocationDataDump[slotIndex] = null;
                    STOLON.Debug.Log($"<skipped slot {slotIndex}.");
                    continue;
                }
                STOLON.Debug.Log($"<found {_entities[_selection[slotIndex]]}.");

                STOLON.Debug.Log($">creating allocation data for slot {slotIndex}..");
                _allocationDataDump[slotIndex] = new EntityAllocationData((int)(100f / usedSlots), _entities[_selection[slotIndex]].GetVirtualAllocation(Selection), _entities[_selection[slotIndex]]);
                STOLON.Debug.Log($"<added to allocdump as; " + _allocationDataDump[slotIndex]);

                STOLON.Debug.Log($">creating allocation DRAW data for slot {slotIndex}..");
                _drawAllocationDataDump[slotIndex] = new EntityDrawAllocationData(secondarySymbolPosOffsetX);
                secondarySymbolPosOffsetX += SYMBOL_NOTATION_SIZE;
                STOLON.Debug.Success();
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
                drawingContext.DrawEntity(SelectedEntity, 512, new Vector2(STOLON.V_WIDTH - 415, 0));

                // draw side black areas.
                drawingContext.DrawArea(new Rectangle(0, 0, _line1x, 1000), Color.Black);
                drawingContext.DrawArea(new Rectangle(_line2x, 0, STOLON.V_WIDTH - _line2x, 1000), Color.Black);

                // draw vertical and horizontal layout lines.
                drawingContext.DrawHorizontalLine(0, INFO_WINDOW_TOPLINE, TILE_SIZE * TILE_ROW_AMOUNT);
                drawingContext.DrawVerticalLine(TILE_SIZE, 0, INFO_WINDOW_TOPLINE);
                drawingContext.DrawVerticalLine(TILE_SIZE * 2, 0, INFO_WINDOW_TOPLINE);
                drawingContext.DrawVerticalLine(TILE_SIZE * 3, 0, INFO_WINDOW_TOPLINE);

                #region ROSTER

                // draw roster lines.
                drawingContext.DrawLine(0, ROSTER_TOP_LINE, TILE_SIZE * TILE_ROW_AMOUNT, ROSTER_TOP_LINE, Color.White, Interface.LINE_WIDTH);
                drawingContext.DrawLine(0, ROSTER_BOTTOM_LINE, TILE_SIZE * TILE_ROW_AMOUNT, ROSTER_BOTTOM_LINE, Color.White, Interface.LINE_WIDTH);

                // draw entity tiles.
                for (int tileIndex = 0; tileIndex < TILE_COUNT; tileIndex++)
                {
                    if (tileIndex < _entityCount)
                    {
                        ref EntityDrawData ddc = ref _entityDrawDump[tileIndex];

                        drawingContext.DrawEntity(ddc.Entity.Profile, TILE_SIZE, ddc.Pos, drawMode: EntityDrawMode.WithBackground);

                        bool isHovered = (_hoveredIndex == tileIndex);
                        bool isSelected = _selection.Contains(tileIndex);

                        if (isHovered)
                        {
                            drawingContext.Draw(STOLON.Textures["UI\\spotlight-128"], ddc.Pos);
                            if (!isSelected) drawingContext.Draw(STOLON.Textures["UI\\selected_add-overlay"], ddc.Pos);
                        }

                        if (isSelected)
                        {
                            int selectedIndex = _selection.GetFirstIndexWhere(x => tileIndex == x);
                            drawingContext.Draw(STOLON.Textures[$"UI\\selected_{selectedIndex + 1}-overlay"], ddc.Pos);
                            if (isHovered) drawingContext.Draw(STOLON.Textures["UI\\selected_remove-overlay"], ddc.Pos);
                        }

                        drawingContext.DrawSymbolNotation(ddc.Entity.SymbolNotation, ddc.SymbolNotationBox);
                        drawingContext.DrawRectangle(new Rectangle(ddc.Pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);

                        if (_lastSelected == tileIndex)
                            drawingContext.Draw(STOLON.Textures["UI\\profile_selected"], ddc.Pos + new Vector2(0, -32));
                    }
                    else
                    {
                        Vector2 pos = GetBaseTilePos(tileIndex);
                        drawingContext.Draw(STOLON.Textures["UI\\profile_question-128"], pos);
                        drawingContext.DrawRectangle(new Rectangle(pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);
                    }
                }

                #endregion

                #region INFO_WINDOW

                drawingContext.Draw(_entityInfoContainer);

                //string wrapped = STOLON.Fonts.Small.Wrap(selectedEntity.Description ?? string.Empty, TILE_SIZE * 2 - 20, INFO_WINDOW_TOPLINE - 12, 0, out int lc).ToUpper();
                //drawingContext.DrawString(STOLON.Fonts.Small, wrapped, new Vector2(10, INFO_WINDOW_TOPLINE - lc * STOLON.Fonts.Small.Dimensions.Y - 10));

                drawingContext.Draw(_allocNotes);

                #region ALLOC_DISPLAYS

                for (int slotIndex = 0; slotIndex < MAX_SELECTION; slotIndex++)
                {
                    if (!IsSlotOccupied(slotIndex)) continue;

                    EntityAllocationData allocData = _allocationDataDump[slotIndex]!.Value;
                    EntityDrawAllocationData drawAllocData = _drawAllocationDataDump[slotIndex]!.Value;

                    string allocationStr = allocData.Allocation.ToString();
                    string virtualAllocStr = allocData.VirtualAllocation.ToString();

                    drawingContext.DrawSymbolNotation(allocData.Entity.SymbolNotation, drawAllocData.SymbolNotationRect);
                    drawingContext.DrawString(STOLON.Fonts.Medium, allocationStr, Centering.Center(STOLON.Fonts.Medium.FastMeasure(allocationStr).ToPoint(), drawAllocData.AllocationRect));
                    drawingContext.DrawString(STOLON.Fonts.Medium, virtualAllocStr, Centering.Center(STOLON.Fonts.Medium.FastMeasure(virtualAllocStr).ToPoint(), drawAllocData.VirtualAllocationRect));

                    if (allocData.VirtualAllocation > allocData.Allocation)
                        drawingContext.Draw(STOLON.Textures["UI\\valloc_inc"], drawAllocData.VirtualAllocationRect);

                    if (_drawConnectionLine)
                        drawingContext.DrawLine(_connectionLine, Color.White, 2);
                }

                #endregion

                //drawingContext.Draw(_boardPreview);
                //drawingContext.Draw(STOLON.Textures["UI\\play"], _boardPreview.Pos + new Vector2(0, -STOLON.Textures["UI\\play"].Height));
                //drawingContext.DrawVerticalLine(_boardPreview.Pos + new Vector2(1, -STOLON.Textures["UI\\play"].Height), STOLON.Textures["UI\\play"].Height, Color.White, 2);

                #endregion

                drawingContext.Draw(_lvlInfoContainer);
            }

            drawingContext.DrawVerticalLine(_line1x, -10f, 1000f);
            drawingContext.DrawVerticalLine(_line2x, -10f, 1000f);
        }
    }
}
