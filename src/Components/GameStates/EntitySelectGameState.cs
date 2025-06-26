using AsitLib.Numerics;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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

            public EntityDrawData(Vector2 basePos, float hoverAmount, Entity? entity)
            {
                Pos = basePos + new Vector2(0, 10 * hoverAmount);
                Entity = entity;
                SymbolNotationBox = new Rectangle(Pos.ToPoint(), new Point(20));
            }

            public bool IsHovered() => new Rectangle(Pos.ToPoint(), new Point(128)).Contains(STOLON.Input.VirtualMousePos);
        }


        private Texture2D _tileTexture;
        private MenuGameState _menuGameState;

        private int _line1x;
        private int _line2x;
        private Tweener<float> _lineTweener;
        private int _hoveredEntityIndex;

        private int _entityCount;

        private bool _initDone;

        private EntityDrawData[] _drawData;
        private float[] _entityHoverdata;
        private readonly Dictionary<int, Vector2> _posCache;

        //private Entity[] _entities;

        public const int TILE_SIZE = 128; // naming conventions for const variables aren't ALL_CAPS? oh no! anyway-
        public const int TILE_ROW_AMOUNT = 4;
        public const int TILE_COLUMN_AMOUNT = 2;
        public const int TILE_COUNT = TILE_ROW_AMOUNT * TILE_COLUMN_AMOUNT;

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
            _entityHoverdata = new float[_entityCount];
            _hoveredEntityIndex = -1;
            _drawData = new EntityDrawData[_entityCount];
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

            _hoveredEntityIndex = -1;
            for (int i = 0; i < _entityCount; i++)
            {
                Vector2 basePos = GetBaseTilePos(i);
                if (new Rectangle(basePos.ToPoint(), new Point(128)).Contains(STOLON.Input.VirtualMousePos))
                {
                    _hoveredEntityIndex = i;
                    _entityHoverdata[i] = MathHelper.Lerp(_entityHoverdata[i], 1, HOVER_INTENSITY);
                }
                else _entityHoverdata[i] = MathHelper.Lerp(_entityHoverdata[i], 0, HOVER_INTENSITY / 5);

                _entityHoverdata[i] = Math.Clamp(_entityHoverdata[i], 0, 1);
                _drawData[i] = new EntityDrawData(basePos, _entityHoverdata[i], _entities[i]);
            }



            //if (_drawData[i].IsHovered() && STOLON.Input.IsClicked(GameInput.MouseButton.Left)) _hoveredEntityIndex = i;

        }

        public Vector2 GetBaseTilePos(int i)
            => _posCache.TryGetValue(i, out Vector2 cachedPos) ? cachedPos :
                _posCache[i] = new Vector2((i % TILE_ROW_AMOUNT) * TILE_SIZE, (TILE_COLUMN_AMOUNT - 1 - i / TILE_ROW_AMOUNT) * TILE_SIZE + (STOLON.V_HEIGHT - 32 - TILE_SIZE * TILE_COLUMN_AMOUNT));

        public override void Draw(DrawingContext drawingContext, int elapsedMilliseconds)
        {
            if (_initDone)
            {
                //drawingContext.Draw(STOLON.Textures.GetReference("Entities\\silo\\silo-512"), new Vector2(448, 0));
                drawingContext.DrawArea(new Rectangle(0, 0, _line1x, 1000), Color.Black);
                for (int i = 0; i < TILE_COUNT; i++)
                {
                    Vector2 pos = GetBaseTilePos(i);
                    if (i < _entityCount)
                    {
                        ref EntityDrawData ddc = ref _drawData[i];
                        drawingContext.DrawEntity(ddc.Profile, TILE_SIZE, ddc.Pos);

                        if (_hoveredEntityIndex == i)
                        {
                            drawingContext.DrawDither(ddc.Pos, new Point(128), 0);
                        }

                        drawingContext.DrawArea(ddc.SymbolNotationBox, Color.Black);
                        drawingContext.DrawRectangle(ddc.SymbolNotationBox, Color.White, UserInterface.LINE_WIDTH);
                        drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], ddc.Entity.SymbolNotation, ddc.Pos + new Vector2(3));
                        drawingContext.DrawRectangle(new Rectangle(ddc.Pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);

                        //drawingContext.DrawArea(new Rectangle(pos.ToPoint() + new Point(0, 100), new Point(STOLON.Fonts[STOLON.MEDIUM_FONT_ID].FastMeasure(_profiles), )), Color.Black);
                        //drawingContext.DrawRectangle(new Rectangle(pos.ToPoint(), new Point(20)), Color.White, UserInterface.LINE_WIDTH);
                        //drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], "Sl", pos + new Vector2(3));
                    }
                    else
                    {
                        drawingContext.Draw(STOLON.Textures["UI\\profile_question-128"], GetBaseTilePos(i));
                        drawingContext.DrawRectangle(new Rectangle(pos.ToPoint(), new Point(TILE_SIZE)), Color.White, 1);
                    }
                }
                //drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], "ENTITY #" + typeof(GoldsilkEntity).GetHashCode(), new Vector2(STOLON.V_WIDTH - 4f, 10f), rotation: 1.57079633f);
            }
            drawingContext.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, UserInterface.LINE_WIDTH);
            drawingContext.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
