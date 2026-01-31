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

        public int GetWindowCount<TWindow>() where TWindow : Window
        {
            return _windows.Count(w => w.GetType() == typeof(TWindow));
        }

        public int GetWindowIndex<TWindow>(TWindow window) where TWindow : Window
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
