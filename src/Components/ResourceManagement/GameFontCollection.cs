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
    public class GameFontCollection : ResourceCollection<GameFont>
    {
        public GameFontCollection(ContentManager contentManager, bool debug = false) : base(contentManager, (toLoad) =>
        {
            try
            {
                return new GameFont(toLoad, contentManager.Load<SpriteFont>(toLoad), toLoad[6..] switch
                {
                    STOLON.SMALL_FONT_ID => 0.5f,
                    _ => 1f,
                });
            }
            catch { return null; }
        }, "Fonts")
        { }
    }
}
