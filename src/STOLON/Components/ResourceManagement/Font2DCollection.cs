using AsitLib;
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
    public class Font2DResourceLoader : SequentialResourceLoader<Font2D>
    {
        public override string[] GetItems() => Directory.GetFiles("Fonts", "*.fnt", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Fonts\\".Length..^".fnt".Length];

        public override Font2D LoadItem(string item)
        {
            using FileStream fileStream = new FileStream(item, FileMode.Open);
            return BitmapFont.FromStream(STOLON.Instance.GraphicsDevice, fileStream, item);
        }
    }

    public class Font2DCollection : ResourceCollection<Font2D>
    {
        public Font2D Small => this["smollerMono"];
        public Font2D Medium => this["pixeloid"];

        public Font2DCollection() : base(new Font2DResourceLoader()) { }
    }
}
