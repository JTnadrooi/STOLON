using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public class WindowShellRegion : ShellRegion
    {
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public override int Height => IsLockActive() ? _window.OuterBounds.Height : _windowSlotTex.Height;

        private Window _window;
        private Texture2D _windowSlotTex;

        private Vector2 _borderCompensatingOffset;

        private bool _isWindowLocked;

        public Window Window => _window;

        public void LockWindow()
        {
            if (_isWindowLocked) throw new InvalidOperationException("Cannot lock already locked window.");

            _isWindowLocked = true;
            _window.IsManaged = false;
        }

        public void UnlockWindow()
        {
            if (!_isWindowLocked) throw new InvalidOperationException("Cannot unlock already unlocked window.");

            _isWindowLocked = false;
            _window.IsManaged = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsLockActive() => _isWindowLocked;

        public WindowShellRegion(Shell shell, Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, Window window) : base(shell)
        {
            if (window.BoundRegion is not null) throw new InvalidOperationException("Cannot bind 'window'; 'window' already belongs to region.");

            _textures = textures;
            _kernel = kernel;

            _window = window;
            _windowSlotTex = textures["UI\\Window\\window_slot"];

            _window.BoundRegion = this;
            LockWindow();
        }

        public override void Update(int elapsedMilliseconds)
        {
            if (_isWindowLocked)
            {
                _window.Position = this.Position;
                _window.Update(elapsedMilliseconds);
            }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            if (_isWindowLocked)
            {
                _window.Draw(drawingContext);
            }
            else
            {
                drawingContext.Draw(_windowSlotTex, Position);
            }

            // window drawing is done by kernel.
        }
    }
}
