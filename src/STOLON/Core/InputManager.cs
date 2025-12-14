using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Diagnostics;
using MonoGame.Extended;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using DiscordRPC;
using DiscordRPC.Events;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace STOLON
{
    public class InputManager
    {
        public enum MouseButton
        {
            Left,
            Middle,
            Right,
        }
        public enum MouseDomain
        {
            OfScreen,
            Board,
            UserInterfaceLow,
            UserInterfaceHigh,
            Dialogue,
        }
        public MouseDomain Domain { get; internal set; }
        public MouseState PreviousMouse { get; internal set; }
        public MouseState CurrentMouse { get; internal set; }
        public KeyboardState CurrentKeyboard { get; internal set; }
        public KeyboardState PreviousKeyboard { get; internal set; }
        public Vector2 VirtualMousePos => Vector2.Transform(CurrentMouse.Position.ToVector2(), STOLON.DrawingContext.InvertYMatrix) / STOLON.Instance.ScreenScale;
        public int MouseScrollValue => CurrentMouse.ScrollWheelValue;
        /// <summary>
        /// Change in scroll value since the last frame (positive = scrolled up, negative = down).
        /// </summary>
        public int MouseScrollDelta => CurrentMouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;
        public bool IsPressed(MouseButton button) => IsPressed(CurrentMouse, button);
        private bool IsPressed(MouseState state, MouseButton button) => button switch
        {
            MouseButton.Left => state.LeftButton,
            MouseButton.Middle => state.MiddleButton,
            MouseButton.Right => state.RightButton,
            _ => throw new Exception(),
        } == ButtonState.Pressed;
        private bool IsPressed(KeyboardState state, Keys key) => state.IsKeyDown(key);
        public bool IsPressed(Keys key) => IsPressed(CurrentKeyboard, key);
        public bool IsClicked(Keys key) => IsPressed(CurrentKeyboard, key) && !IsPressed(PreviousKeyboard, key);
        public bool IsClicked(MouseButton button) => IsPressed(CurrentMouse, button) && !IsPressed(PreviousMouse, button);
    }
}
