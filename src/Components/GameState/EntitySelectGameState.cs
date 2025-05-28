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
        public EntitySelectGameState()
        {
            _tileTexture = STOLON.Textures.GetReference("textures\\temp\\temp_128");
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(_tileTexture, Vector2.Zero, Color.White);
            //spriteBatch.DrawLine(new Vector2(MenuGameState., -10f))
        }

        public void Update(int elapsedMilliseconds)
        {
        }
    }
}
