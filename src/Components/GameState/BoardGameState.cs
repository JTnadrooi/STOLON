using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Point = Microsoft.Xna.Framework.Point;
using Microsoft.Xna.Framework.Content;
using Betwixt;
using MonoGame.Extended;

namespace STOLON
{
    public class BoardGameState : IGameState
    {
        public string DRPStatus => "BoardState";

        private int _lineX1;
        private int _lineX2;
        private float _lineOffset;
        private float _uiLeftOffset;
        private float _uiRightOffset;

        public const int UI_HEIGHT = 1000;

        private Board? _board;
        public Board Board => _board ?? throw new Exception();
        /// <summary>
        /// The virtual X coordiantes of the first line (from left to right).
        /// </summary>
        public float Line1X => _lineX1;
        /// <summary>
        /// The virtual X coordiantes of the second line (from left to right).
        /// </summary>
        public float Line2X => _lineX2;

        public BoardGameState()
        {
            _lineOffset = 192f;
        }

        public void SetBoard(Player[] players) => SetBoard(new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
        public void SetBoard(BoardState state)
        {
            if (BoardState.Validate(state)) _board = new Board(state);
            else throw new Exception();
        }

        public void Update(int elapsedMiliseconds)
        {
            UpdateUI(elapsedMiliseconds);
            _board?.Update(elapsedMiliseconds);
        }
        private void UpdateUI(int elapsedMiliseconds)
        {
            float zoomIntensity = ((BoardGameState)STOLON.StateManager.Current).Board.ZoomIntensity;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            lineZoomOffset = Math.Max(0, lineZoomOffset);

            bool mouseIsOnUI = STOLON.Input.Domain == GameInputManager.MouseDomain.UserInterfaceLow;

            _uiLeftOffset = -lineZoomOffset;
            _uiRightOffset = lineZoomOffset;

            _lineX1 = (int)(_lineOffset + _uiLeftOffset);
            _lineX2 = (int)(STOLON.Instance.VirtualDimensions.X - _lineOffset + _uiRightOffset);
        }
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            _board?.Draw(spriteBatch, elapsedMiliseconds);

            spriteBatch.Draw(STOLON.Textures.Pixel, new Rectangle(Point.Zero, new Point((int)_lineX1, UI_HEIGHT)), Color.Black);
            spriteBatch.DrawLine(_lineX1, -10f, _lineX1, UI_HEIGHT, Color.White, UserInterface.LINE_WIDTH);
            spriteBatch.Draw(STOLON.Textures.Pixel, new Rectangle((int)_lineX2, 0, STOLON.Instance.VirtualDimensions.X - (int)_lineX2, UI_HEIGHT), Color.Black);
            spriteBatch.DrawLine(_lineX2, -10f, _lineX2, UI_HEIGHT, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
