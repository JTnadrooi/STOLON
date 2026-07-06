using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class PlayerWindow : Window
    {
        internal bool IsCurrentPlayer;
        private readonly Board _board;
        private readonly Entity _entity;

        private readonly ITexture2DCollection _textures;

        public PlayerWindow(WindowDependencies deps, Board board, Entity entity) : base(deps, 128, 128)
        {
            _board = board;
            _entity = entity;
            _textures = deps.Textures;

            IsResizable = false;

            Name = entity.Definition.Name + $"(Player {board.State.GetEntityIndex(entity) + 1})";
        }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            drawingContext.Draw(_entity.Definition.Mipmaps[128], Vector2.Zero);

            if (_board.State.CurrentEntity == _entity)
            {
                drawingContext.Draw(_textures["UI\\spotlight_2-128"], Vector2.Zero);
            }
        }
    }
}
