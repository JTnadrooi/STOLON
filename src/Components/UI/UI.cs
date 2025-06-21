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
        private Dictionary<string, UIElement> _AllUIElements;
        private Dictionary<string, UIElementUpdateData> _updateData;

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing all the <see cref="UIElementDrawData"/> objects from all the <see cref="UIElement"/> objects refreched AFTER the UI update.
        /// </summary>
        public Dictionary<string, UIElementUpdateData> UIElementUpdateData => _updateData;
        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all <see cref="UIElement"/> added via the <see cref="AddElement(UIElement)"/> method.
        /// </summary>
        public ReadOnlyDictionary<string, UIElement> UIElements => new ReadOnlyDictionary<string, UIElement>(_AllUIElements);

        public const string TITLE_PARENT_ID = "titleParent";
        private Textframe _textframe;

        /// <summary>
        /// The <see cref="Textframe"/> managed by the <see cref="UserInterface"/>.
        /// </summary>
        public Textframe Textframe => _textframe;

        /// <summary>
        /// The width of a <see cref="UserInterface"/> line.
        /// </summary>
        public int LineWidth => LINE_WIDTH;

        public UIPath MenuPath { get; set; }
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

            _AllUIElements = new Dictionary<string, UIElement>();
            _drawData = new List<UIElementDrawData>();
            _updateData = new Dictionary<string, UIElementUpdateData>();

            STOLON.Debug.Success();
        }
        public void Initialize()
        {
            STOLON.Debug.Log(">[s]initializing ui..");
            // top

            // board l
            //AddElement(new UIElement("exitGame", boardLeftParentId, "Exit Game", UIElementType.Listen));

            //AddElement(new UIElement("screenRegion2", boardLeftParentId, string.Empty, UIElementType.Ignore));
            //AddElement(new UIElement("screenRegion", boardLeftParentId, "Screen & Camera", UIElementType.Ignore));
            //AddElement(new UIElement("toggleFullscreen", boardLeftParentId, "Go Fullscreen", UIElementType.Listen));
            //AddElement(new UIElement("centerCamera", boardLeftParentId, "Center Camera", UIElementType.Listen));

            //AddElement(new UIElement("boardRegion2", boardLeftParentId, string.Empty, UIElementType.Ignore));
            //AddElement(new UIElement("boardRegion", boardLeftParentId, "Board", UIElementType.Ignore));
            //AddElement(new UIElement("undoMove", boardLeftParentId, "Undo", UIElementType.Listen));
            //AddElement(new UIElement("restartBoard", boardLeftParentId, "Restart", UIElementType.Listen));
            //AddElement(new UIElement("boardSearch", boardLeftParentId, "Search", UIElementType.Listen));
            //AddElement(new UIElement("skipMove", boardLeftParentId, "End Move", UIElementType.Listen));

            // board
            //AddElement(new UIElement("currentPlayer", boardRightParentId, null, UIElementType.Ignore));



            STOLON.Debug.Success();
            STOLON.Debug.Success();
        }
        /// <summary>
        /// Clears both the updatedata and drawdata collections, making them ready to be repopulated by the methods in the <see cref="UIOrdering"/> class.<br/>
        /// <i>Does populate the updatedata collection with unhovered <see cref="UIElementDrawData"/> objects.</i>
        /// </summary>
        private void ResetElementData()
        {
            _updateData.Clear();
            foreach (UIElement uiElement in _AllUIElements.Values)
                _updateData.Add(uiElement.Id, new UIElementUpdateData(false, uiElement.Id));
            _drawData.Clear();
        }
        public HashSet<string> GetTopIds() => UIElements.Values.Where(e => e.IsTop).Select(e => e.Id).ToHashSet();
        public HashSet<string> GetParentIds()
        {
            var topIds = GetTopIds();
            return UIElements.Values.WhereSelect(e => (e.ChildOf, !e.IsTop && !topIds.Contains(e.ChildOf))).ToHashSet();
        }
        public override void Update(int elapsedMilliseconds)
        {
            ResetElementData();

            _textframe.Update(elapsedMilliseconds);
        }
        public void PostUpdate(int elapsedMilliseconds)
        {
            foreach (string item in UIElements.Keys)
                if (UIElements[item].Type == UIElementType.Listen)
                {
                    if (_updateData[item].IsClicked)
                        STOLON.Audio.Play(_updateData[item].ClickSound);
                    if (_updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
                        if (updateData2.IsClicked) MenuPath = UIElement.GetParentPath(item);
                }
        }
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public override void Draw(DrawingContext drawingContext, int elapsedMilliseconds)
        {
            string id = STOLON.StateManager.Current.GetId();
            foreach (UIElementDrawData elementDrawData in _drawData)
            {
                drawingContext.DrawString(STOLON.Fonts[elementDrawData.FontName], elementDrawData.Text, elementDrawData.Position);
                if (elementDrawData.DrawRectangle)
                {
                    drawingContext.DrawRectangle(elementDrawData.Rectangle, Color.White, 1f);
                }
            }
            _textframe.Draw(drawingContext, elapsedMilliseconds);
            base.Draw(drawingContext, elapsedMilliseconds);
        }
        /// <summary>
        /// Add an element to the <see cref="UserInterface"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to add.</param>
        public void AddElement(UIElement element)
        {
            _AllUIElements.Add(element.Id, element);
            STOLON.Debug.Log("ui-element with id " + element.Id + " added.");
            //updateData.Add(element.Id, default);
        }
        /// <summary>
        /// Remove an <see cref="UIElement"/> from the <see cref="UserInterface"/>.
        /// </summary>
        /// <param name="elementID">The <see cref="UIElement.Id"/> of the <see cref="UIElement"/> to remove.</param>
        public void RemoveElement(string elementID)
        {
            _AllUIElements.Remove(elementID);
            STOLON.Debug.Log("ui-element with id " + elementID + " removed.");
        }
    }

    public struct UIPath : IEnumerable<string>
    {
        public string TopId => segments[0];
        public string ParentId => segments[^1];
        public string DestinationId => segments.Last();
        public int Lenght => segments.Count;
        public ReadOnlyCollection<string> Segments => segments.AsReadOnly();
        private readonly List<string> segments;
        public UIPath(IEnumerable<string> segments)
        {
            this.segments = new List<string>(segments);
        }
        public string this[int index] => segments[index];
        public IEnumerator<string> GetEnumerator() => segments.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => "{" + segments.ToJoinedString(">") + "}";
        public override int GetHashCode() => segments.ToJoinedString(string.Empty).GetHashCode();
        public override bool Equals([NotNullWhen(true)] object? obj) => obj.GetHashCode() == GetHashCode();
    }

}
