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

            _window.IsDrawnByKernel = false;
            _isWindowLocked = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsLockActive() => _isWindowLocked; //|| (_queuedLockAction.HasValue && _queuedLockAction.Value)

        internal bool TryLockWindow()
        {
            if ((_queuedLockAction.HasValue && _queuedLockAction.Value) || _isWindowLocked) return false;

            _queuedLockAction = true;
            return true;
        }

        internal bool TryUnlockWindow()
        {
            if ((_queuedLockAction.HasValue && !_queuedLockAction.Value) || !_isWindowLocked) return false;

            _queuedLockAction = false;
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

        public Rectangle GetBounds()
        {
            if (_isWindowLocked) return _window.OuterBounds;

            return _windowSlotTex.Bounds.At(Position.ToPoint());
        }

        public override void Update(int elapsedMilliseconds)
        {
            if (_queuedLockAction.HasValue)
            {
                if (_queuedLockAction.Value) // to prevent IsManaged from throwing ex
                {
                    _window.IsDrawnByKernel = false;
                    _isWindowLocked = _queuedLockAction.Value;
                }
                else
                {
                    _isWindowLocked = _queuedLockAction.Value;
                    _window.IsDrawnByKernel = true;
                }

                _queuedLockAction = null;
            }

            if (_isWindowLocked)
            {
                _window.Position = this.Position;
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
