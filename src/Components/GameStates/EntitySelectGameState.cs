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

namespace STOLON
{
    public class EntitySelectGameState : IGameState
    {
        private struct EntityDrawData
        {
            public Vector2 Pos { get; }
            public Rectangle SymbolNotationBox { get; }
            public Rectangle NameBox { get; }
            public EntityProfile? Profile { get; } // should become the actual entity.
            public EntityDrawData(Vector2 pos, EntityProfile? profile)
            {
                Pos = pos;
                Profile = profile;
                SymbolNotationBox = new Rectangle(pos.ToPoint(), new Point(20));
            }
        }

        private GameTexture _tileTexture;
        private MenuGameState _menuGameState;

        private int _line1x;
        private int _line2x;
        private Tweener<float> _lineTweener;

        private bool _initDone;

        private EntityDrawData[] _drawData;

        //private Entity[] _entities;

        public const int TILE_SIZE = 128;
        public const int TILE_ROW_AMOUNT = 4;
        public const int TILE_COLUMN_AMOUNT = 2;
        public const int TILE_COUNT = TILE_ROW_AMOUNT * TILE_COLUMN_AMOUNT;

        public EntitySelectGameState()
        {
            _tileTexture = STOLON.Textures.GetReference("Debug\\temp-" + TILE_SIZE);
            if (!STOLON.StateManager.TryGetState(out _menuGameState!)) throw new Exception();
            _lineTweener = new Tweener<float>(0, 1, 2, Ease.Quad.InOut);
            _lineTweener.Start();

            //_entities = STOLON.Environment.Entities.Values.ToArray();
            EntityProfile[] profiles = STOLON.Environment.GetEntityProfiles().Values.ToArray();
            List<EntityDrawData> tempDrawData = new List<EntityDrawData>();
            for (int i = 0; i < TILE_COUNT; i++)
            {
                int x = (i % TILE_ROW_AMOUNT) * TILE_SIZE;
                int y = (TILE_COLUMN_AMOUNT - 1 - i / TILE_ROW_AMOUNT) * TILE_SIZE;
                tempDrawData.Add(new EntityDrawData(new Vector2(x, y + (STOLON.V_HEIGHT - 32 - TILE_SIZE * TILE_COLUMN_AMOUNT)), profiles.Length > i ? profiles[i] : null));
            }

            _drawData = tempDrawData.ToArray();
        }

        public void Update(int elapsedMilliseconds)
        {
            int To(int orgin, int target, float amount) => (int)(orgin + (target - orgin) * amount);
            _lineTweener.Update(elapsedMilliseconds / 1000f);
            _initDone = !_lineTweener.Running;
            //int line1Target = STOLON.V_WIDTH - (_menuGameState.MenuRemoveLine2x - _menuGameState.MenuRemoveLine1x) + 144;
            int line1Target = TILE_SIZE * TILE_ROW_AMOUNT;
            int line2Target = STOLON.V_WIDTH - 16;

            _line1x = To(_menuGameState.MenuRemoveLine1x, line1Target, _lineTweener.Value);
            _line2x = To(_menuGameState.MenuRemoveLine2x, line2Target, _lineTweener.Value);
        }

        public void Draw(DrawingContext drawingContext, int elapsedMiliseconds)
        {
            if (_initDone)
            {
                //drawingContext.Draw(STOLON.Textures.GetReference("Entities\\silo\\silo-512"), new Vector2(448, 0));
                drawingContext.DrawArea(new Rectangle(0, 0, _line1x, 1000), Color.Black);
                for (int i = 0; i < _drawData.Length; i++)
                {
                    ref EntityDrawData ddc = ref _drawData[i];
                    if (ddc.Profile != null)
                    {
                        drawingContext.Draw(ddc.Profile, 128, ddc.Pos, Vector2.One);

                        drawingContext.DrawArea(ddc.SymbolNotationBox, Color.Black);
                        drawingContext.DrawRectangle(ddc.SymbolNotationBox, Color.White, UserInterface.LINE_WIDTH);
                        drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], "Sl", ddc.Pos + new Vector2(3));

                        //drawingContext.DrawArea(new Rectangle(pos.ToPoint() + new Point(0, 100), new Point(STOLON.Fonts[STOLON.MEDIUM_FONT_ID].FastMeasure(_profiles), )), Color.Black);
                        //drawingContext.DrawRectangle(new Rectangle(pos.ToPoint(), new Point(20)), Color.White, UserInterface.LINE_WIDTH);
                        //drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], "Sl", pos + new Vector2(3));
                    }
                    drawingContext.DrawRectangle(new Rectangle(ddc.Pos.ToPoint(), new Point(128)), Color.White, 1);
                }
                //drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], "ENTITY #" + typeof(GoldsilkEntity).GetHashCode(), new Vector2(STOLON.V_WIDTH - 4f, 10f), rotation: 1.57079633f);
            }
            drawingContext.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, UserInterface.LINE_WIDTH);
            drawingContext.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
