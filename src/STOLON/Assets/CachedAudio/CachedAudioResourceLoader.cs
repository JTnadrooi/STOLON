namespace STOLON
{
    public class CachedAudioResourceLoader : SequentialResourceLoader<CachedAudio>
    {
        public override string[] GetItems() => Directory.GetFiles("Audio", "*.wav", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Audio\\".Length..^".wav".Length];

        public override CachedAudio LoadItem(string item)
        {
            return new CachedAudio(item, GetId(item));
        }
    }
}
