using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsitLib.CommandLine;
namespace STOLON.CLI
{
    public class DiscordCommandProvider : CommandProvider
    {
        private const string DISCORD_INVITE_LINK = @"https://discord.gg/qmuWrqbDG2";

        public DiscordCommandProvider() : base("dc") { }

        [SLCommand("Opens the discord invite link.", IsMain = true)]
        public void Main()
        {
            CLIHelpers.OpenLink(DISCORD_INVITE_LINK);
        }

        [SLCommand("Prints the discord invite link.", Flags = CommandFlags.ReadOnly)]
        public void Link()
        {
            Console.WriteLine(DISCORD_INVITE_LINK);
        }
    }
}
