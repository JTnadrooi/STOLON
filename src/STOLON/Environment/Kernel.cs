using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class Kernel : IComponent, ISingletonDependency
    {
        private readonly IRichLogger _logger;
        private readonly ITexture2DCollection _textures;
        private readonly IInputManager _input;

        private readonly List<Window> _windows;

        public IReadOnlyList<Window> Windows { get; }

        public Kernel(IRichLogger logger, ITexture2DCollection textures, IInputManager input)
        {
            _logger = logger;
            _textures = textures;
            _input = input;

            _windows = new List<Window>();
            Windows = _windows.AsReadOnly();

            //Windows = (_windows = new List<Window>()).AsReadOnly(); // yeah im not doing that, cool though
        }

        internal void RegisterWindow(Window window)
        {
            _windows.Add(window);
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
            return _windows.Where(w => w.GetType() == window.GetType()).IndexOf(window);
        }

        public void Update(int elapsedMilliseconds)
        {
            foreach (Window window in _windows)
            {
                if (window.IsManaged) window.Update(elapsedMilliseconds);
            }
        }

        public void Draw(DrawingContext drawingContext)
        {
            foreach (Window window in _windows)
            {
                if (window.IsManaged) window.Draw(drawingContext);
            }
        }
    }
}
