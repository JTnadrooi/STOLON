using AsitLib.CommandLine;


namespace STOLON.CLI
{
    public class RepositoryCommandProvider : CommandGroup
    {
        public RepositoryCommandProvider() : base("repo", CLI.InfoFactory, nameOfMainMethod: nameof(Main)) { }

        private const string REPO_LINK = "https://github.com/JTnadrooi/STOLON";
        private const string REPO_LINK_GIT = "https://github.com/JTnadrooi/STOLON.git";

        [FlaggedCommand("Opens the main repository page on Github.")]
        public void Main()
        {
            CLIHelpers.OpenLink(REPO_LINK);
        }

        [FlaggedCommand("Prints repository .git link.", Flags = CommandFlags.ReadOnly)]
        public void Link()
        {
            Console.WriteLine(REPO_LINK_GIT);
        }
    }
}
