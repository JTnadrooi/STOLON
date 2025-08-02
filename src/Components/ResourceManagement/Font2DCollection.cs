using AsitLib;
using AsitLib.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Xml.Linq;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;



namespace STOLON
{
    public class Font2DCollection : ResourceCollection<Font2D>
    {
        public const string BASE_PATH = "Fonts";
        public Font2DCollection(ContentManager contentManager, bool debug = false) : base(contentManager, (toLoad) =>
        {
            try
            {
                BitmapFont font = BitmapFont.FromFile(STOLON.Instance.GraphicsDevice, toLoad + ".fnt");
                return new Font2D(toLoad, font);
            }
            catch (Exception e)
            {
                return e;
            }
        }, BASE_PATH)
        { }
        public Font2D Small => this["smollerMono"];
        public Font2D Medium => this["pixeloid"];
    }
}
