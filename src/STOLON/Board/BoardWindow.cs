using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class BoardWindow : Window
    {
        private readonly IFont2DCollection _fonts;
        private readonly IInputManager _input;
        private readonly ILogger _logger;
        private readonly Address _address;
        private readonly Board _board;

        public const int InitialSizeX = 352;
        public const int InitialSizeY = 256;

        public Board Board => _board;

        public BoardWindow(
            WindowDependencies deps,
            ITexture2DCollection textures,
            IFont2DCollection fonts,
            IInputManager input,
            ILogger logger,
            Shell shell,
            Address address) : base(deps, InitialSizeX, InitialSizeY)
        {
            _fonts = fonts;
            _input = input;
            _logger = logger;
            _address = address;

            _board = new Board(textures, fonts, input, logger, shell, address.GetInitialState());

            AddButton(new CloseWindowButton(textures));
            AddButton(new ToggleLockWindowButton(textures));

            BindChildWindow(new ImageWindow(deps, _board.State.Entities[0].Definition));
            BindChildWindow(new ImageWindow(deps, _board.State.Entities[1].Definition));
        }

        protected override void UpdateContents(int elapsedMilliseconds)
        {
            _board.Camera.Dimensions = InnerBounds.Size;
            _board.Update(elapsedMilliseconds);
        }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            _board.Draw(drawingContext);

            drawingContext.DrawPoint(Vector2.Zero, Color.Red, 10);
        }
    }
}
