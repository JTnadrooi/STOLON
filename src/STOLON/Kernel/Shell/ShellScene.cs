using System.Runtime.InteropServices;

namespace STOLON
{
    public class ShellScene : Scene
    {
        private readonly ITexture2DCollection _textures;
        private readonly IRichLogger _logger;
        private readonly Interface _ui;
        private readonly Shell _shell;
        private readonly ITextframe _textframe;
        private readonly IInputManager _input;

        public ShellScene(IRichLogger logger, Interface ui, ITexture2DCollection textures, Shell shell, ITextframe textframe, IInputManager input) : base("shell")
        {
            _logger = logger;
            _ui = ui;
            _textures = textures;
            _shell = shell;
            _textframe = textframe;
            _input = input;

            _textframe.Hide = true;
            _textures = textures;
            //Shell.WriteLine("Hello world!\nWhat a lovelly day.\n> _");
            //Shell.WriteLine("^[s]initialising environment..\r\n    ^---searching for entities..\r\n        ^---called assembly scan for type 'STOLON.Entity\".\r\n        ^---found entity with id 'deceit\" and name 'Deceit\".\r\n        ^---found entity with id 'goldsilk\" and name 'Goldsilk\".\r\n        ^---found entity with id 'north\" and name 'North\".\r\n        ^---found entity with id 'silo\" and name 'Silo\".\r\n        ^---success.\r\n    ^[s]contructing stolon ui..\r\n        ^---success.: time taken: 1ms.\r\n    ^---searching for overlays..\r\n        ^---called assembly scan for type 'STOLON.IOverlay\".\r\n        ^---found overlay with id 'loading\".\r\n        ^---adding overlay of id loading..\r\n            ^---success.\r\n        ^---found overlay with id 'transition_dither\".\r\n        ^---adding overlay of id transition_dither..\r\n            ^---success.\r\n        ^---found overlay with id 'transition\".\r\n        ^---adding overlay of id transition..\r\n            ^---success.\r\n        ^---success.\r\n    ^---success.: time taken: 16ms.\r\n^---success.: time taken: 261ms.".Replace("\r", ""));

            //_shell.WriteLine("Hello. This is a long first line no?");
            //_shell.WriteLine("This is on the second line.");
            //_shell.WriteTexture(_textures["Entities\\north\\north-128"]);
            _shell.HasInputLine = true;
            //_shell.WriteLine("This is on the THIRD line!");
            //_shell.WriteLine("This is on the FOURTH line! (It can't get any crazier than this!1!)");
            //_shell.WriteTexture(_textures["Entities\\north\\north-128"]);
            //_shell.WriteLine("This is on the THIRD line!");
            //_shell.WriteLine("This is on the FOURTH line! (It can't get any crazier than this!1!)");
            //_shell.WriteTexture(_textures["Entities\\north\\north-128"]);
            //_shell.WriteLine("This is on the THIRD line!");
            //_shell.WriteLine("This is on the FOURTH line! (It can't get any crazier than this!1!)");
            //_shell.WriteTexture(_textures["Entities\\north\\north-128"]);

            SiloEntityDefinition siloDefinition = STOLON.Services.Resolve<SiloEntityDefinition>();

            _shell.WriteWindow(new BoardWindow(
                STOLON.Services.Resolve<WindowDependencies>(),
                STOLON.Services.Resolve<ILogger>(),
                STOLON.Services.Resolve<Shell>(),
                STOLON.Services.Resolve<CommandManager>(),
                new A16Address(), siloDefinition.GetDefaultEntity(new UserMoveProvider(_input, STOLON.Services.Resolve<Kernel>())), siloDefinition.GetDefaultEntity(new UserMoveProvider(_input, STOLON.Services.Resolve<Kernel>()))));
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
