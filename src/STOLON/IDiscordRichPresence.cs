namespace STOLON
{
    public interface IDiscordRichPresence
    {
        void DisposeRPC();
        void UpdateDetails(string newDetails);
        void UpdateState(string newState);
    }
}