using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;

namespace STOLON
{
    public class EntitySelectGameState : GameState
    {
        private readonly struct EntityDrawData
        {
            public Vector2 Pos { get; }
            public Rectangle SymbolNotationBox { get; }
            public EntityProfile Profile => Entity.Profile;
            public Entity? Entity { get; }
            public bool Selected { get; }

            public EntityDrawData(Vector2 basePos, float floatAmount, Entity? entity)
            {
                Pos = basePos + new Vector2(0, 10 * floatAmount);
                Entity = entity;
                SymbolNotationBox = new Rectangle(Pos.ToPoint(), new Point(28));
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

        private float[] _entityHoverData;
        private int _hoveredIndex;
        private TimedState<int> _hoveredState;

        private int _selectedIndex;
        private TimedState<int> _selectedState;

        private readonly Dictionary<int, Vector2> _posCache;

        //private Entity[] _entities;

        public const int TILE_SIZE = 128; // naming conventions for const variables aren't ALL_CAPS? oh no! anyway-
        public const int TILE_ROW_AMOUNT = 4;
        public const int TILE_COLUMN_AMOUNT = 2;
        public const int TILE_COUNT = TILE_ROW_AMOUNT * TILE_COLUMN_AMOUNT;

        private const int TILES_CLEARANCE = 32;

        private const float HOVER_INTENSITY = 0.25f;

        private readonly Entity[] _entities;

        public EntitySelectGameState() : base("entity_select")
        {
            _tileTexture = STOLON.Textures.GetReference("Debug\\temp-" + TILE_SIZE);
            if (!STOLON.StateManager.TryGetState(out _menuGameState!)) throw new Exception();
            _lineTweener = new Tweener<float>(0, 1, 2, Ease.Quad.InOut);
            _lineTweener.Start();
            _posCache = new Dictionary<int, Vector2>();

            _entities = STOLON.Environment.Entities.Values.ToArray();
            _entityCount = _entities.Length;
            _entityHoverData = new float[_entityCount];
            _hoveredIndex = -1;
            _selectedIndex = -1;
            _drawData = new EntityDrawData[_entityCount];

            _hoveredState = new TimedState<int>();
            _selectedState = new TimedState<int>();
        }

        protected override void UpdateUI(int elapsedMilliseconds)
        {
            int To(int orgin, int target, float amount) => (int)(orgin + (target - orgin) * amount);
            _lineTweener.Update(elapsedMilliseconds / 1000f);
            _initDone = !_lineTweener.Running;
            int line1Target = TILE_SIZE * TILE_ROW_AMOUNT;
            int line2Target = STOLON.V_WIDTH - 16;

            if (SkipAnimation && _lineTweener.Running) _lineTweener.Update(12f);

            _line1x = To(_menuGameState.MenuRemoveLine1x, line1Target, _lineTweener.Value);
            _line2x = To(_menuGameState.MenuRemoveLine2x, line2Target, _lineTweener.Value);

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
                    _entityHoverData[i] = MathHelper.Lerp(_entityHoverData[i], 1, HOVER_INTENSITY);
                    if (STOLON.Input.IsClicked(GameInput.MouseButton.Left))
                        if (_selectedIndex == i) _selectedIndex = -1;
                        else
                        {
                            _selectedIndex = i;
                            STOLON.Debug.Log("changed selected to " + i + ".");
                        }
                }
                else _entityHoverData[i] = MathHelper.Lerp(_entityHoverData[i], 0, HOVER_INTENSITY / 5);

                _drawData[i] = new EntityDrawData(basePos, _entityHoverData[i], _entities[i]);
            }
        }

        public Vector2 GetBaseTilePos(int i)
            => _posCache.TryGetValue(i, out Vector2 cachedPos) ? cachedPos :
                _posCache[i] = new Vector2((i % TILE_ROW_AMOUNT) * TILE_SIZE, (TILE_COLUMN_AMOUNT - 1 - i / TILE_ROW_AMOUNT) * TILE_SIZE + (STOLON.V_HEIGHT - TILES_CLEARANCE - TILE_SIZE * TILE_COLUMN_AMOUNT));

        public override void Draw(DrawingContext drawingContext, int elapsedMilliseconds)
        {
            if (_initDone)
            {
                //drawingContext.Draw(STOLON.Textures.GetReference("Entities\\silo\\silo-512"), new Vector2(448, 0));
                drawingContext.DrawArea(new Rectangle(0, 0, _line1x, 1000), Color.Black);
                for (int i = 0; i < TILE_COUNT; i++)
                    if (i < _entityCount)
                    {
                        ref EntityDrawData ddc = ref _drawData[i];
                        drawingContext.DrawEntity(ddc.Profile, TILE_SIZE, ddc.Pos);

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
                int lineY = STOLON.V_HEIGHT - TILE_COLUMN_AMOUNT * TILE_SIZE - TILES_CLEARANCE - 32;
                //drawingContext.DrawLine(0, lineY, TILE_SIZE * TILE_ROW_AMOUNT, lineY, Color.White, UserInterface.LINE_WIDTH);
                //drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], "ENTITY #" + typeof(GoldsilkEntity).GetHashCode(), new Vector2(STOLON.V_WIDTH - 4f, 10f), rotation: 1.57079633f);
            }
            drawingContext.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, UserInterface.LINE_WIDTH);
            drawingContext.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
