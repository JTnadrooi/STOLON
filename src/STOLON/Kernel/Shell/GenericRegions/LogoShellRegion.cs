using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class LogoShellRegion : ShellRegion
    {
        private readonly Texture2D _logoMarks;
        private readonly Texture2D _logoFilledMarks;
        private readonly Texture2D _logoFonted;
        private readonly Texture2D _logoLines;

        private Rectangle _logoTileHider;
        private int _logoRowsHidden = 5;
        private const int LogoRowAmount = 5;

        private bool _drawFilledTiles = false;
        private bool _drawFonted = false;
        private bool _flashingComplete = false;
        private int _logoFlashTime = 0;
        private int _millisecondsFlashing = 0;
        private int _millisecondsSinceStartup = 0;

        private const int FlashStart = 1200;
        private const int FlashEnd = 1600;
        private const float Scale = 0.75f;

        private int _width;
        private int _height;
        public override int Width => _width;
        public override int Height => _height;

        public LogoShellRegion(Shell shell, ITexture2DCollection textures) : base(shell)
        {
            _logoMarks = textures.GetReference("UI\\Logo\\Menu\\marks");
            _logoFilledMarks = textures.GetReference("UI\\Logo\\Menu\\filled_marks");
            _logoFonted = textures.GetReference("UI\\Logo\\Menu\\fonted");
            _logoLines = textures.GetReference("UI\\Logo\\Menu\\lines");

            _width = (int)MathF.Ceiling(_logoMarks.Width * Scale);
            _height = (int)MathF.Ceiling(_logoMarks.Height * Scale);
        }

        public override void Update(int elapsedMilliseconds)
        {
            _millisecondsSinceStartup += elapsedMilliseconds;

            if (_millisecondsSinceStartup > 300) _logoRowsHidden = 4;
            if (_millisecondsSinceStartup > 600) _logoRowsHidden = 3;
            if (_millisecondsSinceStartup > 800) _logoRowsHidden = 2;
            if (_millisecondsSinceStartup > 1000) _logoRowsHidden = 1;
            if (_millisecondsSinceStartup > FlashStart) _logoRowsHidden = 0;

            float scaledTotalWidth = _logoLines.Width * Scale;
            float scaledTotalHeight = _logoLines.Height * Scale;
            float rowHeight = scaledTotalHeight / LogoRowAmount;

            float visibleHeight = rowHeight * (LogoRowAmount - _logoRowsHidden);

            int rectX = (int)Position.X;
            int rectY = (int)(Position.Y + visibleHeight);
            int rectWidth = (int)MathF.Ceiling(scaledTotalWidth);
            int rectHeight = (int)MathF.Ceiling(Position.Y + scaledTotalHeight - rectY);

            _logoTileHider = new Rectangle(rectX, rectY, rectWidth, rectHeight);

            if (_millisecondsSinceStartup >= FlashStart && !_flashingComplete)
            {
                if (_millisecondsSinceStartup < FlashStart + 200)
                    _logoFlashTime = 120;
                else if (_millisecondsSinceStartup < FlashStart + 300)
                    _logoFlashTime = 100;
                else if (_millisecondsSinceStartup < FlashStart + 350)
                    _logoFlashTime = 75;
                else if (_millisecondsSinceStartup < FlashEnd)
                    _logoFlashTime = 60;
                else
                    _logoFlashTime = 0;

                if (_logoFlashTime > 0)
                {
                    _millisecondsFlashing += elapsedMilliseconds;
                    if (_millisecondsFlashing >= _logoFlashTime)
                    {
                        _drawFilledTiles = !_drawFilledTiles;
                        _millisecondsFlashing = 0;
                    }
                }
            }

            if (_millisecondsSinceStartup >= FlashEnd && !_flashingComplete)
            {
                _flashingComplete = true;
                _drawFilledTiles = true;
                _drawFonted = true;
                _logoFlashTime = 0;
            }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.Draw(_logoMarks, Position, scale: Scale);

            if (_drawFilledTiles)
                drawingContext.Draw(_logoFilledMarks, Position, scale: Scale);

            if (_drawFonted)
                drawingContext.Draw(_logoFonted, Position, scale: Scale);

            drawingContext.DrawArea(_logoTileHider, Color.Black);

            drawingContext.Draw(_logoLines, Position, scale: Scale);
        }
    }
}