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
            _tileTexture = STOLON.Textures.GetReference("textures\\temp\\temp_" + TILE_SIZE);
            STOLON.StateManager.TryGetState(out _menuGameState!);
            _lineTweener = new Tweener<float>(0, 1, 2, Ease.Quad.InOut);
            _lineTweener.Start();

            _tilePositions = new List<Vector2>();
            for (int i = 0; i < 8; i++)
            {
                _tilePositions.Add(new Vector2(i % TILE_ROW_AMOUNT * TILE_SIZE, i / TILE_ROW_AMOUNT * TILE_SIZE + 32));
            }
        }

        public void Update(int elapsedMilliseconds)
        {
            int To(int orgin, int target, float amount) => (int)(orgin + (target - orgin) * amount);
            _lineTweener.Update(elapsedMilliseconds / 1000f);
            _initDone = !_lineTweener.Running;
            //int line1Target = STOLON.Instance.VirtualDimensions.X - (_menuGameState.MenuRemoveLine2x - _menuGameState.MenuRemoveLine1x) + 144;
            int line1Target = TILE_SIZE * 4;
            int line2Target = STOLON.Instance.VirtualDimensions.X - 32;

            _line1x = To(_menuGameState.MenuRemoveLine1x, line1Target, _lineTweener.Value);
            _line2x = To(_menuGameState.MenuRemoveLine2x, line2Target, _lineTweener.Value);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            if (_initDone)
            {
                spriteBatch.Draw(STOLON.Textures.GetReference("textures\\characters\\silo"), new Vector2(448, 0), Color.White);
                spriteBatch.Draw(STOLON.Textures.Pixel, new Rectangle(0, 0, _line1x, 1000), Color.Black);
                foreach (Vector2 tilePos in _tilePositions)
                {
                    spriteBatch.Draw(_tileTexture, tilePos, Color.White);
                }
            }
            spriteBatch.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, UserInterface.LINE_WIDTH);
            spriteBatch.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
