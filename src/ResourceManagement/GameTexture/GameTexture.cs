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
    public class GameTexture
    {
        public string Name { get => _texture.Name; set => _texture.Name = value; }
        public int Width => _texture.Width;
        public int Height => _texture.Height;
        public Rectangle Bounds => _texture.Bounds;
        public bool IsDisposed => _texture.IsDisposed;

        private Texture2D _texture;
        private bool _disposedValue;

        public GameTexture(Texture2D texture)
        {
            this._texture = texture;
        }
        public GameTexture(GraphicsDevice graphicsDevice, int width, int height, string name = "")
        {
            _texture = new Texture2D(graphicsDevice, width, height);
            _texture.Name = name;
        }
        public void GetColorData(Color[] data) => _texture.GetData(data);
        public void SetColorData(Color[] data) => _texture.SetData(data);
        public static GameTexture GetPixel(GraphicsDevice graphicsDevice, string name = "pixel")
        {
            GameTexture pixel = new GameTexture(graphicsDevice, 1, 1);
            pixel.SetColorData(new Color[] { Color.White });
            pixel.Name = name;
            return pixel;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing) _texture.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public static implicit operator Texture2D(GameTexture t) => t._texture;
        public static explicit operator GameTexture(Texture2D t) => new GameTexture(t);
    }
}
