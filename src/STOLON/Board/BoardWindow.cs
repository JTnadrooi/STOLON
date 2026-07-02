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
            ILogger logger,
            Shell shell,
            Address address, Entity player1, Entity player2) : base(deps, InitialSizeX, InitialSizeY)
        {
            _fonts = deps.Fonts;
            _input = deps.Input;
            _logger = logger;
            _address = address;

            _board = new Board(deps.Textures, deps.Fonts, deps.Input, logger, shell, address.GetInitialState(player1, player2));

            AddButton(new CloseWindowButton(deps.Textures));
            AddButton(new ToggleLockWindowButton(deps.Textures));

            BindChildWindow(new ImageWindow(deps, _board.State.Player1.Definition));
            BindChildWindow(new ImageWindow(deps, _board.State.Player2.Definition));
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
