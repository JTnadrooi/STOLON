using NAudio.Codecs;

namespace STOLON
{
    public class BoardScene : Scene
    {
        private int _lineX1;
        private int _lineX2;
        private float _lineOffset;
        private float _uiLeftOffset;
        private float _uiRightOffset;

        public const int UI_HEIGHT = 1000;

        //private Board? _board;
        //public Board Board => _board ?? throw new Exception();
        /// <summary>
        /// The virtual X coordiantes of the first line (from left to right).
        /// </summary>
        public float Line1X => _lineX1;
        /// <summary>
        /// The virtual X coordiantes of the second line (from left to right).
        /// </summary>
        public float Line2X => _lineX2;

        private readonly SceneManager _sceneManager;

        public BoardScene(SceneManager sceneManager) : base("board")
        {
            _sceneManager = sceneManager;

            _lineOffset = 192f;
        }

        public void SetBoard(Player[] players) => SetBoard(new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
        public void SetBoard(BoardState state)
        {
            //if (BoardState.Validate(state)) _board = new Board(state);
            //else
            throw new Exception();
        }

        protected override void UpdateEnvironment(int elapsedMilliseconds)
        {
            UpdateUI(elapsedMilliseconds);
            //_board?.Update(elapsedMilliseconds);
        }
        protected override void UpdateUI(int elapsedMilliseconds)
        {
            //float zoomIntensity = ((BoardScene)_sceneManager.Current).Board.ZoomIntensity;
            float zoomIntensity = 1;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            lineZoomOffset = Math.Max(0, lineZoomOffset);

            //bool mouseIsOnUI = _input.Domain == InputManager.MouseDomain.UserInterfaceLow;

            _uiLeftOffset = -lineZoomOffset;
            _uiRightOffset = lineZoomOffset;

            _lineX1 = (int)(_lineOffset + _uiLeftOffset);
            _lineX2 = (int)(STOLON.VWidth - _lineOffset + _uiRightOffset);
        }
        public override void Draw(DrawingContext drawingContext)
        {
            //_board?.Draw(drawingContext);

            drawingContext.DrawArea(new Rectangle(Point.Zero, new Point((int)_lineX1, UI_HEIGHT)), Color.Black);
            drawingContext.DrawLine(_lineX1, -10f, _lineX1, UI_HEIGHT, Color.White, Interface.LineWidth);
            drawingContext.DrawArea(new Rectangle((int)_lineX2, 0, STOLON.VWidth - (int)_lineX2, UI_HEIGHT), Color.Black);
            drawingContext.DrawLine(_lineX2, -10f, _lineX2, UI_HEIGHT, Color.White, Interface.LineWidth);
        }
    }
}
