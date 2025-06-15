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
        private GameTexture _tileTexture;
        private MenuGameState _menuGameState;

        private int _line1x;
        private int _line2x;
        private Tweener<float> _lineTweener;

        private bool _initDone;

        private List<Vector2> _tilePositions;

        public const int TILE_SIZE = 128;
        public const int TILE_ROW_AMOUNT = 4;
        public const int TILE_COLUMN_AMOUNT = 2;

        public EntitySelectGameState()
        {
            _tileTexture = STOLON.Textures.GetReference("Debug\\temp-" + TILE_SIZE);
            if (!STOLON.StateManager.TryGetState(out _menuGameState!)) throw new Exception();
            _lineTweener = new Tweener<float>(0, 1, 2, Ease.Quad.InOut);
            _lineTweener.Start();

            _tilePositions = new List<Vector2>();
            for (int i = 0; i < TILE_ROW_AMOUNT * TILE_COLUMN_AMOUNT; i++)
            {
                _tilePositions.Add(new Vector2(i % TILE_ROW_AMOUNT * TILE_SIZE, i / TILE_ROW_AMOUNT * TILE_SIZE + (STOLON.V_HEIGHT - 32 - TILE_SIZE * TILE_COLUMN_AMOUNT)));
            }
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
                //drawingContext.Draw(STOLON.Textures.GetReference("characters\\silo"), new Vector2(448, 0), Color.White);
                drawingContext.DrawArea(new Rectangle(0, 0, _line1x, 1000), Color.Black);
                foreach (Vector2 tilePos in _tilePositions)
                {
                    drawingContext.Draw(_tileTexture, tilePos);
                }
                //drawingContext.DrawString(STOLON.Fonts[STOLON.MEDIUM_FONT_ID], "ENTITY #" + typeof(GoldsilkEntity).GetHashCode(), new Vector2(STOLON.V_WIDTH - 4f, 10f), rotation: 1.57079633f);
            }
            drawingContext.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, UserInterface.LINE_WIDTH);
            drawingContext.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
