using AsitLib.CommandLine;
namespace STOLON.CLI
{
    public class DiscordCommandProvider : CommandGroup
    {
        private const string DISCORD_INVITE_LINK = @"https://discord.gg/qmuWrqbDG2";

        public DiscordCommandProvider() : base("dc", CLI.InfoFactory, nameOfMainMethod: nameof(Main)) { }

        [FlaggedCommand("Opens the discord invite link.")]
        public void Main()
        {
            CLIHelpers.OpenLink(DISCORD_INVITE_LINK);
        }

        [FlaggedCommand("Prints the discord invite link.", Flags = CommandFlags.ReadOnly)]
        public void Link()
        {
            Console.WriteLine(DISCORD_INVITE_LINK);
        }
    }
}
