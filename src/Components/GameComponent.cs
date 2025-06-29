using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Linq;
using System.Collections.Generic;
using System;
using System.Runtime.Versioning;
using System.Reflection.Metadata;
using AsitLib;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Collections;


namespace STOLON
{
    /// <summary>
    /// A interface that provides a basic way to interact with <see cref="Intrara"/> component classes.
    /// </summary>
    public interface IGameComponent
    {
        public Vector2 Position { get; }
        /// <summary>
        /// Update this component so it computes all the calculations.
        /// </summary>
        /// <param name="elapsedMilliseconds">The milliseconds since last frame.</param>
        public void Update(int elapsedMilliseconds);
        /// <summary>
        /// Update this component so it draws all sub-drawables.
        /// </summary>
        /// <param name="elapsedMilliseconds">The milliseconds since last frame.</param>
        public void Draw(DrawingContext drawingContext);
    }
    public abstract class GameComponent : IGameComponent
    {
        public virtual Vector2 Position { get; protected set; }
        public IGameComponent? Source { get; }

        protected GameComponent(IGameComponent? source = null)
        {
            Position = Vector2.Zero;
            Source = source;
        }
        public virtual void Draw(DrawingContext drawingContext)
        {

        }
        public virtual void Update(int elapsedMilliseconds)
        {

        }
    }

}
