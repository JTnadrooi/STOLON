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

        private readonly Dictionary<Type, Window> _singleInstanceWindows;

        public Kernel(IRichLogger logger, ITexture2DCollection textures, IInputManager input, CommandManager commandManager)
        {
            _logger = logger;
            _textures = textures;
            _input = input;

            _windows = new List<Window>();
            _drawingOrder = new List<Window>();
            _singleInstanceWindows = new Dictionary<Type, Window>();

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
            if (window.IsSingleInstance)
            {
                Type windowType = window.GetType();

                if (_singleInstanceWindows.ContainsKey(windowType))
                {
                    throw new InvalidOperationException("Single instance window already registered.");
                }

                _singleInstanceWindows.Add(windowType, window);
            }

            _windows.Add(window);
            //if (!)
            //throw new ArgumentException("Window is already registered.");
            _drawingOrder.Add(window); // Initially same order

        }

        private void DeregisterWindow(Window window, int index)
        {
            _drawingOrder.Remove(window);
            _windows.RemoveAt(index);

            Type windowType = window.GetType();
            _singleInstanceWindows.Remove(windowType);
        }

        //internal void DeregisterWindow(Window window)
        //{
        //    _windows.Remove(window);
        //    _drawingOrder.Remove(window);
        //}

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

        public TWindow GetWindow<TWindow>() where TWindow : Window
        {
            Type windowType = typeof(TWindow);

            if (_singleInstanceWindows.TryGetValue(windowType, out Window? windowFromCache))
            {
                return (TWindow)windowFromCache;
            }
            else if (_windows.TryGetFirst(w => w is TWindow, out Window? windowFromWindows))
            {
                return (TWindow)windowFromWindows;
            }
            else throw new InvalidOperationException($"Window with type '{windowType.ToString()}' not registerd.");
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
            for (int i = 0; i < _windows.Count; i++)
            {
                Window window = _windows[i];

                window.Update(elapsedMilliseconds);

                if (_input.Keyboard.IsClicked(Keys.C) && _input.Keyboard.IsPressed(Keys.LeftControl))
                {
                    window.Close();
                }

                if (window.Status == WindowStatus.PendingClosed)
                {
                    //DeregisterWindow(_windows[i]);
                    window.TryUnlock();

                    DeregisterWindow(window, i);
                    i--;

                    window.SetStatus(WindowStatus.PendingClosed);
                }
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
