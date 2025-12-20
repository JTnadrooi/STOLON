using DiscordRPC;


namespace STOLON
{
    public class DiscordRichPresence
    {
        public DiscordRpcClient _client;
        private RichPresence _presence;

        public DiscordRichPresence()
        {
            _client = new DiscordRpcClient("1291994415207944255");
            //client.Logger = new ConsoleLogger()
            //{
            //	Level = LogLevel.Warning,
            //};
            _client.Initialize();
            _presence = new RichPresence()
            {
                Details = "STOLON",
                State = string.Empty,
                Assets = new Assets()
                {
                    LargeImageKey = "stolonicon",
                    LargeImageText = "STOLON",
                    SmallImageKey = string.Empty,
                }
            };
            _client.SetPresence(_presence);
        }

        public void UpdateState(string newState)
        {
            _presence.State = newState;
            _client.SetPresence(_presence);
        }

        public void UpdateDetails(string newDetails)
        {
            _presence.Details = newDetails;
            _client.SetPresence(_presence);
        }

        public void DisposeRPC() => _client.Dispose();
    }
}
