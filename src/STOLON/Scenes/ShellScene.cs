namespace STOLON
{
    public class ShellScene : Scene
    {
        public Shell Shell { get; }

        public ShellScene() : base("shell")
        {
            STOLON.UI.Textframe.Hide = true;

            Shell = new Shell();
            //Shell.WriteLine("Hello world!\nWhat a lovelly day.\n> _");
            //Shell.WriteLine("^[s]initialising environment..\r\n    ^---searching for entities..\r\n        ^---called assembly scan for type 'STOLON.Entity\".\r\n        ^---found entity with id 'deceit\" and name 'Deceit\".\r\n        ^---found entity with id 'goldsilk\" and name 'Goldsilk\".\r\n        ^---found entity with id 'north\" and name 'North\".\r\n        ^---found entity with id 'silo\" and name 'Silo\".\r\n        ^---success.\r\n    ^[s]contructing stolon ui..\r\n        ^---success.: time taken: 1ms.\r\n    ^---searching for overlays..\r\n        ^---called assembly scan for type 'STOLON.IOverlay\".\r\n        ^---found overlay with id 'loading\".\r\n        ^---adding overlay of id loading..\r\n            ^---success.\r\n        ^---found overlay with id 'transition_dither\".\r\n        ^---adding overlay of id transition_dither..\r\n            ^---success.\r\n        ^---found overlay with id 'transition\".\r\n        ^---adding overlay of id transition..\r\n            ^---success.\r\n        ^---success.\r\n    ^---success.: time taken: 16ms.\r\n^---success.: time taken: 261ms.".Replace("\r", ""));

            Shell.WriteLine("Hello. This is a long first line no?");
            Shell.WriteLine("This is on the second line.");
        }

        protected override void UpdateUI(int elapsedMilliseconds)
        {
            Shell.Update(elapsedMilliseconds);
        }

        public override void Draw(DrawingContext drawingContext)
        {
            drawingContext.Draw(STOLON.Textures["Entities\\north\\north-128"], new Microsoft.Xna.Framework.Vector2(500, 200));
            Shell.Draw(drawingContext);
        }
    }
}
