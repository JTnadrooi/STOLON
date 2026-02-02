
namespace STOLON
{
    [Dependency(ServiceLifetime.Transient)]
    public class CachedAudioResourceLoader : SequentialResourceLoader<CachedAudio>
    {
        public CachedAudioResourceLoader(IRichLogger logger) : base(logger)
        {
        }

        public override string[] GetItems() => Directory.GetFiles("Audio", "*.wav", SearchOption.AllDirectories);

        public override string GetId(string item) => item["Audio\\".Length..^".wav".Length];

        public override CachedAudio LoadItem(string item)
        {
            return new CachedAudio(item, GetId(item));
        }
    }
}
