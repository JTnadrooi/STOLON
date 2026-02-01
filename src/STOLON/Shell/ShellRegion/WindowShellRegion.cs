using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace STOLON
{
    public class WindowShellRegion : ShellRegion
    {
        private readonly ITexture2DCollection _textures;
        private readonly Kernel _kernel;

        public override int Height => IsLockActive() ? _window.OuterBounds.Height : _windowSlotTex.Height;
        public Window Window => _window;

        private Window _window;
        private Texture2D _windowSlotTex;
        private Vector2 _borderCompensatingOffset;

        private bool _isWindowLocked;
        private bool? _queuedLockAction;

        public WindowShellRegion(Shell shell, Kernel kernel, ITexture2DCollection textures, IFont2DCollection fonts, IInputManager input, Window window) : base(shell)
        {
            if (window.BoundRegion is not null) throw new InvalidOperationException("Cannot bind 'window'; 'window' already belongs to region.");

            _textures = textures;
            _kernel = kernel;

            _window = window;
            _windowSlotTex = textures["UI\\Window\\window_slot"];

            _window.BoundRegion = this;

            _window.IsManaged = false;
            _isWindowLocked = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsLockActive() => _isWindowLocked;

        internal void LockWindow()
        {
            if (_queuedLockAction.GetValueOrDefault() || _isWindowLocked) throw new InvalidOperationException("Cannot lock already locked window.");

            _queuedLockAction = true;
        }

        internal void UnlockWindow()
        {
            if (_queuedLockAction.GetValueOrDefault() || !_isWindowLocked) throw new InvalidOperationException("Cannot unlock already unlocked window.");

            _queuedLockAction = false;
        }

        public override void Update(int elapsedMilliseconds)
        {
            if (_queuedLockAction.HasValue)
            {
                if (_queuedLockAction.Value) // to prevent IsManaged from throwing ex
                {
                    _window.IsManaged = !_queuedLockAction.Value;
                    _isWindowLocked = _queuedLockAction.Value;
                }
                else
                {
                    _isWindowLocked = _queuedLockAction.Value;
                    _window.IsManaged = !_queuedLockAction.Value;
                }

                _queuedLockAction = null;
            }

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
            else  // window drawing is done by kernel.
            {
                drawingContext.Draw(_windowSlotTex, Position);
            }
        }
    }
}
