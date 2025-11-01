using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class DiscordCommandProvider : CommandProvider
    {
        private const string DISCORD_INVITE_LINK = @"https://discord.gg/qmuWrqbDG2";

        public DiscordCommandProvider() : base("dc") { }

        [Command("Opens the discord invite link.")]
        public void _M()
        {
            CLIHelpers.OpenLink(DISCORD_INVITE_LINK);
        }

        [Command("Print discord invite link.", flags: CommandFlag.ReadOnly)]
        public void Link()
        {
            Console.WriteLine(DISCORD_INVITE_LINK);
        }
    }
}
