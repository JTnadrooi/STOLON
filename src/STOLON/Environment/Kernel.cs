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

        private List<Window> _windows;

        public Kernel(IRichLogger logger, ITexture2DCollection textures)
        {
            _logger = logger;
            _textures = textures;

            _windows = new List<Window>();
        }

        public void RegisterWindow(Window window)
        {
            _windows.Add(window);
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
