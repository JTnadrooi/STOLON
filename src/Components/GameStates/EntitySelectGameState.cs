using AsitLib;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public void PrepareOrdering(Vector2 origin, int elementCount)
        {
            _origin = origin;
            _leftSpace = 0;
        }

        public UIElementDrawData GetDrawData(UIElement element, int index, out bool isHovered)
        {
            Vector2 pos = _origin + new Vector2(_leftSpace, 0);
            Rectangle bounds = element.GetBounds(pos.ToPoint(), PADDING_X, PADDING_Y, 5, (int)(BOXED_TEXT_DIV_CLEARANCE / 2 - _font.Dimensions.Y / 2 - PADDING_Y), out Point textPos);
            _leftSpace += bounds.Width + 5;

            isHovered = false;
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

        private string _counterStr;
        private Vector2 _counterPos;

        private readonly record struct CachedNoteData(string WrappedText, int LineCount, bool IsActive, Texture2D NoteSign);

        private const int NOTE_CLEARANCE = 12;
        private const int NOTE_BORDER_X_CLEARANCE = 10;

        public ConditionalNoteEnumerationGraphic(EntitySelectGameState entitySelect, Vector2 pos, int textWidth)
        {
            Notes = Array.Empty<ConditionalNote>();
            Pos = pos;
            TextWidth = textWidth;
            _counterStr = string.Empty;
            _cachedNotes = Array.Empty<CachedNoteData>();
            _entitySelect = entitySelect;
        }

        public void Update(int elapsedMilliseconds)
        {
            int activePosCount = 0;
            int activeNegCount = 0;
            int totalPosCount = 0;
            int totalNegCount = 0;

            if (Notes.Length != _cachedNotes.Length) _cachedNotes = new CachedNoteData[Notes.Length];
            for (int i = 0; i < Notes.Length; i++)
            {
                bool isActive = Notes[i].IsActive(_entitySelect.Selection);
                void Count(ref int active, ref int total)
                {
                    if (isActive) active++;
                    total++;
                }
                Texture2D noteSign;
                var note = Notes[i];
                bool isPosOrNeutral = note.IsPositiveOrNeutral;

                if (isActive)
                    noteSign = STOLON.Textures[
                        isPosOrNeutral ? "UI\\note_sign_pos_enabled" : "UI\\note_sign_neg_enabled"
                    ];
                else noteSign = STOLON.Textures["UI\\note_sign_disabled"];

                if (isPosOrNeutral) Count(ref activePosCount, ref totalPosCount);
                else Count(ref activeNegCount, ref totalNegCount);

                _cachedNotes[i] = new CachedNoteData(STOLON.Fonts.Small.Wrap(Notes[i].Text, TextWidth - NOTE_BORDER_X_CLEARANCE * 2 - NOTE_CLEARANCE, int.MaxValue, out var lc).ToUpper(),
                    lc,
                    isActive,
                    noteSign
                );
            }

            _counterStr = $"[{activePosCount}/{totalPosCount}] / [{activeNegCount}/{totalNegCount}]";
            _counterPos = Centering.CenterX((int)STOLON.Fonts.Small.FastMeasure(_counterStr).X, 10, TILE_SIZE) + new Vector2(Pos.X, 0);
        }

        public void Draw(DrawingContext drawingContext)
        {
            int notesClearingUp = 7;
            int noteSpacing = STOLON.Fonts.Small.CoreFont.LineHeight / 2;

            for (int i = 0; i < _cachedNotes.Length; i++)
            {
                CachedNoteData note = _cachedNotes[i];
                drawingContext.DrawString(STOLON.Fonts.Small, note.WrappedText, new Vector2((int)Pos.X + NOTE_BORDER_X_CLEARANCE + NOTE_CLEARANCE, (int)Pos.Y - notesClearingUp - note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight));
                //drawingContext.DrawString(STOLON.Fonts.Small, "-", new Vector2((int)Pos.X + NOTE_BORDER_X_CLEARANCE, (int)Pos.Y - notesClearingUp - STOLON.Fonts.Small.CoreFont.LineHeight));
                drawingContext.Draw(note.NoteSign, new Vector2((int)Pos.X + NOTE_BORDER_X_CLEARANCE, (int)Pos.Y - notesClearingUp - STOLON.Fonts.Small.CoreFont.LineHeight));

                if (note.IsActive)
                    drawingContext.DrawRectangle(new Rectangle((int)Pos.X + 4, (int)Pos.Y - notesClearingUp - note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight - 3, TextWidth - 8, note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight + 6), thickness: 1);

                notesClearingUp += note.LineCount * STOLON.Fonts.Small.CoreFont.LineHeight + noteSpacing;
            }

            drawingContext.Draw(STOLON.Textures["UI\\dotted_line-128"], new Vector2(Pos.X, STOLON.Fonts.Small.CoreFont.LineHeight + 20 - 1));

            drawingContext.DrawString(STOLON.Fonts.Small, _counterStr, _counterPos);
        }
    }

    public class EntitySelectGameState : GameState
    {
        #region SUBSTRUCTS

        private readonly struct EntityDrawData // for per-frame updates.
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
        private readonly struct SelectedEntityDrawData // for selected entities.
        {
            public readonly Rectangle SymbolNotationRect;
            public readonly Rectangle AllocationRect;
            public readonly Rectangle VirtualAllocationRect;
            public readonly int VirtualAllocation;
            public readonly int Allocation;
            public readonly string SymbolNotation;

            public SelectedEntityDrawData(EntitySelectGameState gameState, Entity entity)
                : this(gameState, gameState.Selection.GetSlot(entity.Id), entity.SymbolNotation, gameState.Selection.GetAllocation(entity.Id), gameState.Selection.GetVirtualAllocation(entity.Id))
            { }
            public SelectedEntityDrawData(EntitySelectGameState gameState, int slotIndex, string symbolNotation, int alloc, int valloc)
            {
                Rectangle GetMiniSlot(int ySlotIndex) => new Rectangle(TILE_SIZE * 2 + SYMBOL_NOTATION_SIZE * slotIndex + (int)gameState._symbolNotationOffset, INFO_WINDOW_TOPLINE - 32 - SYMBOL_NOTATION_SIZE * ySlotIndex, SYMBOL_NOTATION_SIZE, SYMBOL_NOTATION_SIZE);

                SymbolNotationRect = GetMiniSlot(0);
                AllocationRect = GetMiniSlot(1);
                VirtualAllocationRect = GetMiniSlot(2);
                SymbolNotation = symbolNotation;

                Allocation = alloc;
                VirtualAllocation = valloc;
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
        private List<SelectedEntityDrawData> _drawAllocationDataDump;

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
        private ConditionalNoteEnumerationGraphic _abilityNotes;

        private BoardState _boardState;
        private BoardPreview _boardPreview;
        private bool _drawConnectionLine;
        private Line _connectionLine;

        private int _symbolNotationOffsetTarget;
        private float _symbolNotationOffset;
        private (string sn, int alloc, int valloc)? _ghostSelectedEntityDrawDataTemplate; // hmmm

        public EntitySelection Selection { get; private set; }

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

            _drawAllocationDataDump = new List<SelectedEntityDrawData>(MAX_SELECTION);

            _hoveredState = new TimedState<int>();
            _selectedState = new TimedState<int>();
            _boardState = BoardState.GetDefault([new Player("player0"), STOLON.Environment.Entities["goldsilk"].GetPlayer()]);
            _boardPreview = _boardState.GetPreview();

            _symbolNotationOffsetTarget = 0;

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
            _abilityNotes = new ConditionalNoteEnumerationGraphic(this, new Vector2(TILE_SIZE, INFO_WINDOW_TOPLINE), TILE_SIZE);

            if (SkipArgs != null)
            {
                _lastSelected = SkipArgs[0] != "-1" ? _entities.GetFirstIndexWhere(e => e.Id == SkipArgs[0]) : _lastSelected;
            }

            Selection = new EntitySelection(MAX_SELECTION);

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

            _symbolNotationOffsetTarget = (int)((MAX_SELECTION - Selection.Count) * SYMBOL_NOTATION_SIZE * 0.5f);
            _symbolNotationOffset = MathHelper.Lerp(_symbolNotationOffset, _symbolNotationOffsetTarget, 0.1f);
            if (Math.Abs(_symbolNotationOffset - _symbolNotationOffsetTarget) < (Selection.Count > 0 ? 0.1f : 1f)) _symbolNotationOffset = _symbolNotationOffsetTarget;
            //Console.WriteLine(_symbolNotationOffsetTarget + " " + _symbolNotationOffset + " " + Selection.Count);

            #region TILES

            for (int entityIndex = 0; entityIndex < _entityCount; entityIndex++)
            {
                Vector2 basePos = GetBaseTilePos(entityIndex);
                Rectangle entityRect = new Rectangle(basePos.ToPoint(), new Point(128));
                string entityId = _entities[entityIndex].Id;

                bool isHovered = entityRect.Contains(STOLON.Input.VirtualMousePos);
                _entityHoverCoefficients[entityIndex] = MathHelper.Lerp(_entityHoverCoefficients[entityIndex], isHovered ? 1 : 0, isHovered ? HOVER_INTENSITY : HOVER_INTENSITY / 5);

                if (isHovered)
                {
                    _hoveredIndex = entityIndex;

                    Rectangle addBox = new Rectangle(basePos.ToPoint() + new Point(0, TILE_SIZE - ADD_BOX_SIZE), new Point(32));

                    if (addBox.Contains(STOLON.Input.VirtualMousePos) && STOLON.Input.IsClicked(GameInput.MouseButton.Left))
                        if (Selection.Contains(entityId))
                        {
                            _ghostSelectedEntityDrawDataTemplate = (_entities[entityIndex].SymbolNotation, Selection[entityId].Allocation, Selection[entityId].VAllocation);
                            Selection.Remove(entityId);
                        }
                        else
                        {
                            _ghostSelectedEntityDrawDataTemplate = null;
                            Selection.Add(entityId);
                        }
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

            _drawAllocationDataDump.Clear();
            for (int slotIndex = 0; slotIndex < MAX_SELECTION; slotIndex++)
                if (slotIndex < Selection.Count) _drawAllocationDataDump.Add(new SelectedEntityDrawData(this, Selection[slotIndex].Entity));
                else if (_ghostSelectedEntityDrawDataTemplate.HasValue)
                    _drawAllocationDataDump.Add(new SelectedEntityDrawData(this, slotIndex, _ghostSelectedEntityDrawDataTemplate.Value.sn, _ghostSelectedEntityDrawDataTemplate.Value.alloc, _ghostSelectedEntityDrawDataTemplate.Value.valloc));

            for (int i = 0; i < Selection.Count; i++)
            {
                if (_hoveredIndex != -1 && (_entities[_hoveredIndex].Id == Selection[i].Entity.Id || _drawAllocationDataDump[i].SymbolNotationRect.Contains(STOLON.Input.VirtualMousePos)))
                {
                    _connectionLine = new Line(_entityDrawDump[_entities.GetFirstIndexWhere(e => e == Selection[i].Entity)].Pos.ToPoint() + new Point(TILE_SIZE / 2, 0), _drawAllocationDataDump[i].SymbolNotationRect.Location + new Point(SYMBOL_NOTATION_SIZE / 2, SYMBOL_NOTATION_SIZE));
                    _drawConnectionLine = true;
                }
            }

            #endregion

            const int BOARDPREVIEW_CLEARANCE = 0;
            _boardPreview.Pos = new Vector2(LINE1_TARGET - BOARDPREVIEW_CLEARANCE - _boardPreview.Dimensions.X, INFO_WINDOW_TOPLINE - BOARDPREVIEW_CLEARANCE - _boardPreview.Dimensions.Y);

            // update selected entity UI panel.
            _currentEntitySelectedCoefficient = MathHelper.Lerp(_currentEntitySelectedCoefficient, 1, HOVER_INTENSITY / 2);
            _entityInfoContainer.Position = new Vector2((_currentEntitySelectedCoefficient - 1) * 200, INFO_WINDOW_TOPLINE);

            _entityInfoContainer.Elements["extended_name"].Text = _entityDrawDump[_lastSelected].FullerName;
            _entityInfoContainer.Elements["alloc"].Text = $"(alloc) {(Selection.GetAllocation(_entities[_lastSelected].Id))}%";
            _entityInfoContainer.Elements["v_alloc"].Text = $"(valloc) {(Selection.GetVirtualAllocation(_entities[_lastSelected].Id))}%";
            _entityInfoContainer.Update(elapsedMilliseconds);

            // update level info container.
            _lvlInfoContainer.Elements["lvl_name"].Text = "STOLON Test Level";
            _lvlInfoContainer.Elements["lvl_diff"].Text = "Difficulty 1";
            _lvlInfoContainer.Update(elapsedMilliseconds);

            _allocNotes.Notes = SelectedEntity.AllocationNotes;
            _allocNotes.Update(elapsedMilliseconds);

            _abilityNotes.Notes = SelectedEntity.AbilityNotes;
            _abilityNotes.Update(elapsedMilliseconds);
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
                        bool isSelected = Selection.Contains(ddc.Entity.Id);

                        if (isHovered)
                        {
                            drawingContext.Draw(STOLON.Textures["UI\\spotlight-128"], ddc.Pos);
                            if (!isSelected) drawingContext.Draw(STOLON.Textures["UI\\selected_add-overlay"], ddc.Pos);
                        }

                        if (isSelected)
                        {
                            int selectedIndex = Selection.GetSlot(ddc.Entity.Id);
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

                #region INFO_WINDOW (basically everything bellow the roster)

                drawingContext.Draw(_entityInfoContainer);

                //string wrapped = STOLON.Fonts.Small.Wrap(selectedEntity.Description ?? string.Empty, TILE_SIZE * 2 - 20, INFO_WINDOW_TOPLINE - 12, 0, out int lc).ToUpper();
                //drawingContext.DrawString(STOLON.Fonts.Small, wrapped, new Vector2(10, INFO_WINDOW_TOPLINE - lc * STOLON.Fonts.Small.Dimensions.Y - 10));

                drawingContext.Draw(_allocNotes);
                drawingContext.Draw(_abilityNotes);

                #region ALLOC_DISPLAYS

                int selectedEntityIndex = 0;
                for (int i = 0; i < _drawAllocationDataDump.Count; i++)
                {
                    SelectedEntityDrawData drawAllocData = _drawAllocationDataDump[i];
                    //if (!IsSlotOccupied(slotIndex) || (selectedEntityIndex > 2 && (int)_symbolNotationOffset != 0))

                    string allocationStr = drawAllocData.Allocation.ToString();
                    string virtualAllocStr = drawAllocData.VirtualAllocation.ToString();

                    drawingContext.DrawSymbolNotation(drawAllocData.SymbolNotation, drawAllocData.SymbolNotationRect);
                    drawingContext.DrawString(STOLON.Fonts.Medium, allocationStr, Centering.Center(STOLON.Fonts.Medium.FastMeasure(allocationStr).ToPoint(), drawAllocData.AllocationRect));
                    drawingContext.DrawString(STOLON.Fonts.Medium, virtualAllocStr, Centering.Center(STOLON.Fonts.Medium.FastMeasure(virtualAllocStr).ToPoint(), drawAllocData.VirtualAllocationRect));

                    if (drawAllocData.VirtualAllocation > drawAllocData.Allocation) drawingContext.Draw(STOLON.Textures["UI\\valloc_inc"], drawAllocData.VirtualAllocationRect);

                    if (_drawConnectionLine) drawingContext.DrawLine(_connectionLine, Color.White, 2);

                    selectedEntityIndex++;
                }

                int deltaVAlloc = Selection.TotalVAllocation - 100;
                string deltaVAllocStr = deltaVAlloc >= 0 ? "+" + deltaVAlloc : deltaVAlloc.ToString();
                Vector2 deltaVAllocPos = new Vector2(TILE_SIZE * 2, 0) + Centering.CenterX((int)STOLON.Fonts.Medium.FastMeasure(deltaVAllocStr).X, 10, TILE_SIZE);
                drawingContext.DrawString(STOLON.Fonts.Medium, deltaVAllocStr, deltaVAllocPos);
                //if (deltaVAlloc > 0) drawingContext.Draw(STOLON.Textures["UI\\valloc_inc"], deltaVAllocPos);

                drawingContext.DrawArea(new Rectangle(TILE_SIZE * 2, 0, (int)_symbolNotationOffset, INFO_WINDOW_TOPLINE), Color.White); // curtain 1.
                drawingContext.DrawArea(new Rectangle((int)(TILE_SIZE * 3 - _symbolNotationOffset), 0, (int)_symbolNotationOffset + 1, INFO_WINDOW_TOPLINE), Color.White); // curtain 2.

                #endregion

                #region LVL_INFO

                drawingContext.DrawArea(new Rectangle(TILE_SIZE * 3, 0, TILE_SIZE * 2, INFO_WINDOW_TOPLINE), Color.Black);

                #endregion

                //drawingContext.Draw(_boardPreview);
                //drawingContext.Draw(STOLON.Textures["UI\\play"], _boardPreview.Pos + new Vector2(0, -STOLON.Textures["UI\\play"].Height));
                //drawingContext.DrawVerticalLine(_boardPreview.Pos + new Vector2(1, -STOLON.Textures["UI\\play"].Height), STOLON.Textures["UI\\play"].Height, Color.White, 2);

                #endregion

                // draw vertical and horizontal layout lines.
                drawingContext.DrawHorizontalLine(0, INFO_WINDOW_TOPLINE, TILE_SIZE * 3);
                drawingContext.DrawVerticalLine(TILE_SIZE, 0, INFO_WINDOW_TOPLINE);
                drawingContext.DrawVerticalLine(TILE_SIZE * 2, 0, INFO_WINDOW_TOPLINE);

                drawingContext.DrawVerticalLine(TILE_SIZE * 3, 0, ROSTER_BOTTOM_LINE);

                drawingContext.Draw(_lvlInfoContainer);
            }

            drawingContext.DrawVerticalLine(_line1x, -10f);
            drawingContext.DrawVerticalLine(_line2x, -10f);
        }
    }
}
