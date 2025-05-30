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
    public interface IMipmapped
    {
        public GameTexture? Texture32x => null;
        public GameTexture? Texture64x => null;
        public GameTexture? Texture96x => null;
        public GameTexture? Texture128x => null;
        public GameTexture? Texture256x => null;
        public GameTexture? Texture512x => null;
    }

    public static class MipmappedExtensions
    {
        public static GameTexture GetMipmap(this IMipmapped graphicsItem, int res) => res switch
        {
            32 => graphicsItem.Texture32x,
            64 => graphicsItem.Texture64x,
            96 => graphicsItem.Texture96x,
            128 => graphicsItem.Texture128x,
            256 => graphicsItem.Texture256x,
            512 => graphicsItem.Texture512x,
            _ => throw new ArgumentException($"Unsupported resolution: {res}")
        } ?? throw new InvalidOperationException($"Texture for resolution {res} is not set (null).");
    }

}
