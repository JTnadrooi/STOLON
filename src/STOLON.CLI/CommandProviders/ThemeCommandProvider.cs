using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class ThemeCommandProvider : CommandGroup
    {
        private readonly IConfiguration _config;

        public ThemeCommandProvider(IConfiguration config) : base("theme", CLI.InfoFactory, nameOfMainMethod: nameof(Main))
        {
            _config = config;
        }

        [FlaggedCommand("Prints the name of the color theme.", Flags = CommandFlags.ReadOnly)]
        public void Main()
        {
            Console.WriteLine(_config.Get<string>("graphics.theme"));
        }

        [FlaggedCommand("Sets the color theme.")]
        public void Set(string themeId)
        {
            _config.Set("graphics.theme", themeId);
        }

        [FlaggedCommand("Sets the color theme.")]
        public void Reset(string themeId)
        {
            _config.Reset("graphics.theme");
        }

        [FlaggedCommand("Prints the path of the color theme.")]
        public void Path(string? themeId = null)
        {
            throw new NotImplementedException();
        }
    }
}
