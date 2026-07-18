using MonoGame.Extended.ECS;
using NAudio.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class CornerBlocksOrderContainer : OrderContainer<TextureElement>
    {
        public const int ButtonSize = 11;
        private readonly IInputManager _input;

        public CornerBlocksOrderContainer(IInputManager input, IEnumerable<TextureElement> elements, Vector2 position, int columns) : base(input, elements, position)
        {
            _input = input;
        }

        protected override bool VerifyElement(TextureElement element)
        {
            return base.VerifyElement(element) && element.Texture is not null && element.Texture.Height == ButtonSize && element.Texture.Width == ButtonSize;
        }

        public override UIElementDrawData GetDrawData(TextureElement element, int orderIndex, out bool isHovered)
        {
            int x = (int)Position.X + (orderIndex % 2 == 0 ? (ButtonSize - 1) : 0);
            int y = (int)Position.Y + (int)(orderIndex / 2f) * (ButtonSize - 1) - 1;

            Vector2 pos = new Vector2(x, y);
            Rectangle bounds = new Rectangle((int)pos.X, (int)pos.Y, ButtonSize, ButtonSize);

            isHovered = bounds.Contains(_input.Mouse.GetTransformedMousePosition(Transform));
            if (isHovered) Console.WriteLine("AAA");
            return new UIElementDrawData(element, element.Texture!, element.Type, pos, bounds, false);
        }
    }

    public class PlayerWindow : Window
    {
        internal bool IsCurrentPlayer;
        private readonly Board _board;
        private readonly IInputManager _input;
        private readonly Entity _entity;

        private readonly ITexture2DCollection _textures;
        private readonly CornerBlocksOrderContainer _cornerBlocksOrderContainer;

        private const int Size = 128;
        private const int CornerBlocksOrderContainerColumnCount = 2;

        public PlayerWindow(WindowDependencies deps, Board board, Entity entity) : base(deps, Size, Size)
        {
            _board = board;
            _input = deps.Input;
            _entity = entity;
            _textures = deps.Textures;

            _cornerBlocksOrderContainer = new CornerBlocksOrderContainer(deps.Input, [
                new TextureElement("activate_ability1", deps.Textures["UI\\Window\\window_button_unlock"]),
                new TextureElement("activate_ability2", deps.Textures["UI\\Window\\window_button_unlock"]),
                new TextureElement("activate_ability3", deps.Textures["UI\\Window\\window_button_unlock"]),
            ], new Vector2(Size - CornerBlocksOrderContainer.ButtonSize * CornerBlocksOrderContainerColumnCount + CornerBlocksOrderContainerColumnCount, 0), CornerBlocksOrderContainerColumnCount);

            IsResizable = false;

            Name = entity.Definition.Name + $"(Player {board.State.GetEntityIndex(entity) + 1})";

            AddButton(new CloseWindowButton(deps.Textures));
        }

        protected override void UpdateContents(int elapsedMilliseconds)
        {
            _cornerBlocksOrderContainer.Transform = Transform;
            _cornerBlocksOrderContainer.Update(elapsedMilliseconds);
            //Console.WriteLine(_input.MouseOn);

            Name = _entity.Definition.Name + $"(Player {_board.State.GetEntityIndex(_entity) + 1})";
            if (_board.State.CurrentEntity == _entity)
            {
                Name += " (Current)";
            }
        }

        protected override void DrawContents(DrawingContext drawingContext)
        {
            drawingContext.Draw(_entity.Definition.Mipmaps[128], Vector2.Zero);

            _cornerBlocksOrderContainer.Draw(drawingContext);
        }
    }
}
