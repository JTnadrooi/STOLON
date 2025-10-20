using Microsoft.Xna.Framework;
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
using System.Collections;
using AsitLib.Collections;
using System.Diagnostics.CodeAnalysis;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.IO;
using MonoGame.Extended.Content;
using Microsoft.Xna.Framework.Content;




namespace STOLON
{
    public class Texture2DCollection : ContentCollection<Texture2D>
    {
        private readonly Texture2D _pixel;

        public Texture2D Pixel => _pixel;

        public Texture2DCollection(ContentManager contentManager, bool debug = false) : base(contentManager, (toLoad) =>
        {
            try
            {
                Texture2D texture = contentManager.Load<Texture2D>(toLoad);
                if (debug)
                {
                    Color[] data = new Color[texture.Width * texture.Height];
                    texture.GetData(data);
                }
                return texture;
            }
            catch (Exception e)
            {
                return e;
            }
        }, "Textures")
        {
            _pixel = new Texture2D(contentManager.GetGraphicsDevice(), 1, 1);
            ((Texture2D)_pixel).SetData([Color.White]);
        }
        public override void UnloadContent()
        {
            _pixel.Dispose();
            base.UnloadContent();
        }
    }
}
