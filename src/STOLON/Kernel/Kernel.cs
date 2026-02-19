using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    [Dependency(ServiceLifetime.Singleton)]
    public sealed class Kernel : IComponent
    {
        private readonly IRichLogger _logger;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;

        public IReadOnlyList<Window> Windows { get; }

        private readonly List<Window> _windows;
        private readonly List<Window> _drawingOrder;

        public Kernel(IRichLogger logger, ITexture2DCollection textures, IInputManager input, CommandManager commandManager)
        {
            _logger = logger;
            _textures = textures;
            _input = input;

            _windows = new List<Window>();
            _drawingOrder = new List<Window>();

            Windows = _windows;

            //Windows = (_windows = new List<Window>()).AsReadOnly(); // not doing that, cool though
        }

        private void ThrowIfNotRegistered(Window window)
        {
            if (!_windows.Contains(window))
                throw new ArgumentException("Window is not registered to the kernel.", nameof(window)); // currently not possible but just in case.
        }

        internal void RegisterWindow(Window window)
        {
            _windows.Add(window);
            //if (!)
            //throw new ArgumentException("Window is already registered.");
            _drawingOrder.Add(window); // Initially same order
        }

        internal bool Focus(Window window)
        {
            ThrowIfNotRegistered(window);

            if (_drawingOrder[_drawingOrder.Count - 1] == window)
            {
                return false;
            }

            _drawingOrder.Remove(window);
            _drawingOrder.Add(window);

            return true;
        }

        /// <summary>
        /// Returns the amount of windows registered to the kernel of exact type <typeparamref name="TWindow"/>.
        /// </summary>
        /// <typeparam name="TWindow">The type of window to count.</typeparam>
        /// <returns>The amount of windows registered to the kernel of exact type <typeparamref name="TWindow"/>.</returns>
        public int GetCount<TWindow>() where TWindow : Window
        {
            return _windows.Count(w => w.GetType() == typeof(TWindow));
        }

        /// <summary>
        /// Gets the index of the specified window within the collection of windows of the same exact type <typeparamref name="TWindow"/>.
        /// </summary>
        /// <typeparam name="TWindow">The type of window to search for.</typeparam>
        /// <param name="window">The window instance to find.</param>
        /// <returns>
        /// The index of the window within the filtered collection of windows of the same type as <paramref name="window"/>. Returns -1 if not found.
        /// </returns>
        public int GetIndex<TWindow>(TWindow window) where TWindow : Window
        {
            ThrowIfNotRegistered(window);

            return _windows.Where(w => w.GetType() == window.GetType()).IndexOf(window);
        }

        public void Update(int elapsedMilliseconds)
        {
            foreach (Window window in _windows)
            {
                window.Update(elapsedMilliseconds);
            }
        }

        public void Draw(DrawingContext drawingContext)
        {
            foreach (Window window in _drawingOrder)
            {
                if (window.IsDrawnByKernel) window.Draw(drawingContext);
            }
        }
    }
}
