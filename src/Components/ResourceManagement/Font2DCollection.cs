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
                return new Font2D(toLoad, contentManager.Load<BitmapFont>(toLoad), toLoad switch
                {
                    //BASE_PATH + "\\smoller" => 0.5f,
                    _ => 1f,
                });
            }
            catch (Exception e)
            {
                return e;
            }
        }, BASE_PATH)
        { }
        public Font2D Small => this["smoller"];
        public Font2D Medium => this["pixeloid"];
    }
}
