using System.Runtime.InteropServices;

namespace STOLON
{
    public class ShellScene : Scene
    {
        private readonly ITexture2DCollection _textures;
        private readonly IRichLogger _logger;
        private readonly Shell _shell;
        private readonly ITextframe _textframe;
        private readonly IInputManager _input;

        public ShellScene(IRichLogger logger, ITexture2DCollection textures, Shell shell, ITextframe textframe, IInputManager input) : base("shell")
        {
            _logger = logger;
            _textures = textures;
            _shell = shell;
            _textframe = textframe;
            _input = input;

            _textframe.Hide = true;
            _textures = textures;
            _shell.HasInputLine = true;

            SiloEntityDefinition siloDefinition = STOLON.Services.Resolve<SiloEntityDefinition>();

            _shell.Command("sadr a16 silo silo");
            _shell.Command("stadr");
        }

        protected override void UpdateInterface(int elapsedMilliseconds)
        {
            if (_input.IsPressed(Keys.LeftControl))
            {
                if (_input.IsClicked(Keys.W)) _shell.WriteLine(new Random().Next().ToString());
                if (_input.IsClicked(Keys.E)) _shell.Write(new Random().Next().ToString());
            }

            _shell.Update(elapsedMilliseconds);
        }

        public override void Draw(DrawingContext drawingContext)
        {
            //drawingContext.Draw(_textures["Entities\\north\\north-128"], new Vector2(500, 200));
            _shell.Draw(drawingContext);
        }
    }
}
