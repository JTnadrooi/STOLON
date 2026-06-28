using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public class WindowShellRegion : ShellRegion
    {
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public override int Height => IsLockActive() ? _window.OuterBounds.Height : _windowSlotTex.Height;
        public override int Width => IsLockActive() ? _window.OuterBounds.Width : _windowSlotTex.Width;
        public Window? Window => _window;

        private Window? _window;
        private Texture2D _windowSlotTex;
        private Texture2D _windowClosedTex;
        private Vector2 _borderCompensatingOffset;

        private bool _isWindowLocked;

        public WindowShellRegion(Shell shell, Kernel kernel, ITexture2DCollection textures, Window window) : base(shell)
        {
            if (window.BoundRegion is not null) throw new InvalidOperationException("Cannot bind 'window'; 'window' already belongs to region.");

            _textures = textures;
            _kernel = kernel;

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
            if (_isWindowLocked)
            {
                _window.Position = Position;
            }

            if (_window.Status == WindowStatus.Closed)
            {
                _window = null;
                _isWindowLocked = false;
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
                    drawingContext.Draw(_windowSlotTex, Position);
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
