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
        public string DRPStatus => nameof(EntitySelectGameState);

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
        }

        public void Update(int elapsedMilliseconds)
        {
        }
    }
}
