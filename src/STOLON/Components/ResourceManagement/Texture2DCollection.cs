using AsitLib;
using AsitLib.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Windows;
using System.Xml.Linq;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace STOLON
{
    public class Texture2DResourceLoader : SequentialResourceLoader<Texture2D>
    {
        public override string[] GetItems() => Directory.GetFiles("Textures", "*.png", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Textures\\".Length..^".png".Length];

        public override Texture2D LoadItem(string item)
        {
            using FileStream fileStream = new FileStream(item, FileMode.Open);
            return Texture2D.FromStream(STOLON.Instance.GraphicsDevice, fileStream);
        }
    }

    public class Texture2DCollection : ResourceCollection<Texture2D>
    {
        private Texture2D? _pixel;

        public Texture2D Pixel => _pixel ?? throw new Exception();

        public Texture2DCollection() : base(new Texture2DResourceLoader()) { }

        public override void LoadResources()
        {
            _pixel = new Texture2D(STOLON.Instance.GraphicsDevice, 1, 1);
            ((Texture2D)_pixel).SetData([Color.White]);
            base.LoadResources();
        }

        public override void UnloadResources()
        {
            _pixel.Dispose();
            base.UnloadResources();
        }
    }
}
