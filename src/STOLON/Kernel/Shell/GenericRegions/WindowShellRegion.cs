using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace STOLON
{
    public class WindowShellRegion : ShellRegion
    {
        private readonly ITexture2DCollection _textures;
        private readonly IFont2DCollection _fonts;
        private readonly Kernel _kernel;

        public override int Height => IsLockActive() ? _window.OuterBounds.Height : _windowSlotTex.Height;
        public override int Width => IsLockActive() ? _window.OuterBounds.Width : _windowSlotTex.Width;
        public Window? Window => _window;
        public bool IsWindowClosed => _window is null;

        private Window? _window;
        private Texture2D _windowSlotTex;
        private Texture2D _windowClosedTex;
        private Vector2 _borderCompensatingOffset;
        private Vector2 _windowSlotNamePos;

        private bool _isWindowLocked;
        private Font2D _font;

        public WindowShellRegion(Shell shell, Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, Window window) : base(shell)
        {
            if (window.BoundRegion is not null) throw new InvalidOperationException("Cannot bind 'window'; 'window' already belongs to region.");

            _textures = textures;
            _fonts = fonts;
            _kernel = kernel;

            _font = fonts.Medium;

            _window = window;
            _windowSlotTex = textures["UI\\Window\\window_slot"];
            _windowClosedTex = textures["UI\\Window\\window_closed"];

            _window.BoundRegion = this;

            _window.IsDrawnByKernel = false;
            _isWindowLocked = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsLockActive() => _isWindowLocked; //|| (_queuedLockAction.HasValue && _queuedLockAction.Value)

        internal bool TryLockWindow()
        {
            if (_isWindowLocked) return false;

            _window.IsDrawnByKernel = false;
            _isWindowLocked = true;

            return true;
        }

        internal bool TryUnlockWindow()
        {
            if (!_isWindowLocked) return false;

            _isWindowLocked = false;
            _window.IsDrawnByKernel = true;

            return true;
        }

        internal void LockWindow()
        {
            if (!TryLockWindow()) throw new InvalidOperationException("Cannot lock already locked window.");
        }

        internal void UnlockWindow()
        {
            if (!TryUnlockWindow()) throw new InvalidOperationException("Cannot unlock already unlocked window.");
        }

        public override void Update(int elapsedMilliseconds)
        {
            if (_window?.Status == WindowStatus.Closed)
            {
                _window = null;
                _isWindowLocked = false;
            }
            else if (_window?.Status == WindowStatus.Open)
            {
                Vector2 textSize = _font.FastMeasure(_window.Name);
                _windowSlotNamePos = Centering.Center(textSize.ToPoint(), _windowSlotTex.Bounds.At(Position.ToPoint()));
                _windowSlotNamePos += new Vector2(0, -1);
                NumberHelper.OnPixel(ref _windowSlotNamePos);
            }

            if (_isWindowLocked)
            {
                _window.Position = Position;
            }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            if (_isWindowLocked)
            {
                _window.Draw(drawingContext);
            }
            else  // window drawing is done by kernel, so just draw slot
            {
                if (_window is null) // window closed
                {
                    drawingContext.Draw(_windowClosedTex, Position);
                }
                else
                {
                    drawingContext.Draw(_windowSlotTex, Position);
                    drawingContext.DrawString(_font, _window.Name, _windowSlotNamePos);
                }
            }
        }

        public override void PostDraw(DrawingContext drawingContext)
        {
            if (_isWindowLocked)
            {
                _window.PostDraw(drawingContext);
            }
        }
    }
}
