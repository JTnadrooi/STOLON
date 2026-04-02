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

        public BoardWindow(
            WindowDependencies deps,
            ITexture2DCollection textures,
            IFont2DCollection fonts,
            IInputManager input,
            ILogger logger,
            Address address) : base(deps, 256, 256)
        {
            _fonts = fonts;
            _input = input;
            _logger = logger;
            _address = address;

            _board = new Board(textures, fonts, input, logger, address.GetInitialBoardState());


            AddButton(new ToggleLockWindowButton(textures));
        }

        protected override void UpdateContents(int elapsedMilliseconds)
        {
            _board.Update(elapsedMilliseconds);
        }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            _board.Draw(drawingContext);

            drawingContext.DrawPoint(Vector2.Zero, Color.Red, 10);
        }
    }
}
