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
    public interface IResourceCollection<TContent> : IDisposable, IReadOnlyDictionary<string, TContent>
    {
        public ContentManager ContentManager { get; }
        public TContent GetReference(string path);
        public void UnloadContent();
    }
    public abstract class ResourceCollection<TContent> : IResourceCollection<TContent>
    {
        protected readonly Dictionary<string, TContent> dictionary = new Dictionary<string, TContent>();
        private bool _disposedValue;
        public string BasePath { get; }

        public ContentManager ContentManager { get; }

        public IEnumerable<string> Keys => dictionary.Keys;
        public IEnumerable<TContent> Values => dictionary.Values;
        public int Count => dictionary.Count;
        public TContent this[string key] => dictionary[key];

        public const bool SILENT = false;

        public ResourceCollection(ContentManager contentManager, Func<string, object> loader, string basePath)
        {
            BasePath = basePath;
            string[] files = Directory.GetFiles(contentManager.RootDirectory, "*", SearchOption.AllDirectories);
            if (files.Length == 0) throw new Exception("No initial content found.");

            STOLON.Debug.Log($">loading data with basePath: '{basePath}\" for type {this.GetType().Name}.");

            foreach (string file in files)
            {
                string toLoad = file[(contentManager.RootDirectory.Length + 1)..].Split('.')[..^1].ToJoinedString();
                STOLON.Debug.Log("found file: " + toLoad);
                if (!toLoad.StartsWith(basePath)) continue;
                string toLoadId = toLoad[(basePath.Length + 1)..];
                STOLON.Debug.Log(">attempting load of resource with id/key: " + toLoad);
                object loaderResult = loader(toLoad);
                if (loaderResult is Exception)
                {
                    STOLON.Debug.Log("<failed, exception: " + (SILENT ? "SILENT" : (loaderResult as Exception)));
                }
                else
                {
                    dictionary[toLoadId] = (TContent)loaderResult;
                    STOLON.Debug.Log("added with id: " + toLoadId);
                    STOLON.Debug.Success();
                }
            }
            STOLON.Debug.Log(dictionary.Values.Count + " assets loaded.");
            STOLON.Debug.Success();
            ContentManager = contentManager;
        }

        //public virtual void Add(TContent resource, string? newName = null) => dictionary.Add(newName, resource);

        public virtual TContent GetReference(string path) => this[path];

        public virtual void UnloadContent()
        {
            foreach (TContent item in dictionary.Values)
                if (item is IDisposable disposable)
                    disposable.Dispose();
            dictionary.Clear();
        }

        public bool ContainsKey(string key) => dictionary.ContainsKey(key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TContent value) => dictionary.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, TContent>> GetEnumerator() => dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue && disposing)
            {
                UnloadContent();
                _disposedValue = true;
            }
        }

        ~ResourceCollection() => Dispose(disposing: false);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
