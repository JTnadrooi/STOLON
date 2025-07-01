using AsitLib;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using static STOLON.UIElement;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;



namespace STOLON
{
    /// <summary>
    /// The user interface for the <see cref="GameEnvironment"/>.
    /// </summary>
    public class UserInterface : GameComponent
    {
        private List<UIElementDrawData> _drawData;
        public List<UIElementDrawData> DrawData => _drawData;

        public const int LINE_WIDTH = 2;
        private Dictionary<string, UIElement> _elements;
        private Dictionary<string, UIElementUpdateData> _updateData;

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing all the <see cref="UIElementDrawData"/> objects from all the <see cref="UIElement"/> objects refreched AFTER the UI update.
        /// </summary>
        public Dictionary<string, UIElementUpdateData> UpdateData => _updateData;
        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all <see cref="UIElement"/> added via the <see cref="AddElement(UIElement)"/> method.
        /// </summary>
        public ReadOnlyDictionary<string, UIElement> Elements => _elements.AsReadOnly();

        private Textframe _textframe;

        /// <summary>
        /// The <see cref="Textframe"/> managed by the <see cref="UserInterface"/>.
        /// </summary>
        public Textframe Textframe => _textframe;

        /// <summary>
        /// The width of a <see cref="UserInterface"/> line.
        /// </summary>
        public int LineWidth => LINE_WIDTH;

        /// <summary>
        /// Main UIInterface contructor.
        /// </summary>
        public UserInterface() : base(STOLON.Environment)
        {
            string CamelCase(string s)
            {
                string x = s.Replace("_", "");
                if (x.Length == 0) return "null";
                x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                    m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
                return char.ToLower(x[0]) + x.Substring(1);
            }
            STOLON.Debug.Log(">[s]contructing stolon ui");

            _textframe = new Textframe(this);

            STOLON.Debug.Log(">loading audio");
            foreach (string filePath in Directory.GetFiles("audio", "*.wav", SearchOption.AllDirectories))
            {
                string fileName = CamelCase(Path.GetFileNameWithoutExtension(filePath).Replace(" ", string.Empty));
                STOLON.Audio.Library.Add(fileName, new CachedAudio(filePath, fileName));
                STOLON.Debug.Log("loaded audio with id: " + fileName);
            }
            STOLON.Debug.Success();

            _elements = new Dictionary<string, UIElement>();
            _drawData = new List<UIElementDrawData>();
            _updateData = new Dictionary<string, UIElementUpdateData>();

            STOLON.Debug.Success();
        }
        public override void Update(int elapsedMilliseconds)
        {
            _textframe.Update(elapsedMilliseconds);
        }
        //public void PostUpdate(int elapsedMilliseconds)
        //{
        //    foreach (string item in Elements.Keys)
        //        if (Elements[item].Type == UIElementType.Listen)
        //        {
        //            if (_updateData[item].IsClicked)
        //                STOLON.Audio.Play(_updateData[item].ClickSound);
        //            if (_updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
        //                if (updateData2.IsClicked) MenuPath = UIElement.GetParentPath(item);
        //        }
        //}
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public override void Draw(DrawingContext drawingContext)
        {
            foreach (UIElementDrawData elementDrawData in _drawData)
                drawingContext.DrawElement(elementDrawData);
            _textframe.Draw(drawingContext);
            base.Draw(drawingContext);
        }
    }

    public struct UIPath : IEnumerable<string>
    {
        public string TopId => _segments[0];
        public string ParentId => _segments[^1];
        public string DestinationId => _segments.Last();
        public int Lenght => _segments.Length;
        public ReadOnlySpan<string> Segments => _segments;
        private readonly string[] _segments;
        public UIPath(IEnumerable<string> segments) => _segments = segments.ToArray();
        public string this[int index] => _segments[index];
        public IEnumerator<string> GetEnumerator() => (IEnumerator<string>)_segments.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => "{" + _segments.ToJoinedString(">") + "}";
        public override int GetHashCode() => _segments.ToJoinedString(string.Empty).GetHashCode();
        public override bool Equals([NotNullWhen(true)] object? obj) => obj.GetHashCode() == GetHashCode();
    }

}
