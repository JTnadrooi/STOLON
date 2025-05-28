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

        public const int TILE_SIZE = 128;

        public EntitySelectGameState()
        {
            _tileTexture = STOLON.Textures.GetReference("textures\\temp\\temp_" + TILE_SIZE);
            STOLON.StateManager.TryGetState(out _menuGameState!);
            _lineTweener = new Tweener<float>(0, 1, 2, Ease.Quad.InOut);
            _lineTweener.Start();
        }

        public void Update(int elapsedMilliseconds)
        {
            _lineTweener.Update(elapsedMilliseconds / 1000f);
            int menuLineRemoveDelta = TILE_SIZE * 2 - _menuGameState.MenuRemoveLine1x;
            int lineOffset = (int)(_menuGameState.MenuRemoveLine1x + menuLineRemoveDelta * _lineTweener.Value);

            _line1x = lineOffset;
            _line2x = STOLON.Instance.VirtualDimensions.X - lineOffset;
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(_tileTexture, Vector2.Zero, Color.White);
            spriteBatch.Draw(_tileTexture, new Vector2(TILE_SIZE, 0), Color.White);
            spriteBatch.Draw(STOLON.Textures.GetReference("textures\\characters\\silo"), new Vector2(500, 0), Color.White);
            spriteBatch.DrawLine(_line1x, -10f, _line1x, 1000f, Color.White, UserInterface.LINE_WIDTH);
            spriteBatch.DrawLine(_line2x, -10f, _line2x, 1000f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
