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
        public const int LINE_WIDTH = 2;

        private Textframe _textframe;

        /// <summary>
        /// The <see cref="Textframe"/> managed by the <see cref="UserInterface"/>.
        /// </summary>
        public Textframe Textframe => _textframe;
        public DefaultDictionary<string, UIElementUpdateData> UpdateDump => _updateData;
        private readonly DefaultDictionary<string, UIElementUpdateData> _updateData;

        /// <summary>
        /// The width of a <see cref="UserInterface"/> line.
        /// </summary>
        public int LineWidth => LINE_WIDTH;

        /// <summary>
        /// Main UIInterface contructor.
        /// </summary>
        public UserInterface() : base(STOLON.Environment)
        {
            STOLON.Debug.Log(">[s]contructing stolon ui");

            _textframe = new Textframe(this);
            _updateData = new DefaultDictionary<string, UIElementUpdateData>(s => new UIElementUpdateData(false, null));


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
