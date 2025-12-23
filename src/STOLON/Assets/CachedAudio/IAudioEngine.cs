using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace STOLON
{
    public interface IAudioEngine
    {
        Playlist? Current { get; }
        DictionaryMixingSampleProvider FXMixer { get; }
        float FxVolume { get; set; }
        MixingSampleProvider MasterMixer { get; }
        float MasterVolume { get; set; }
        DictionaryMixingSampleProvider OSTMixer { get; }
        float OstVolume { get; set; }

        void AddMixerInput(ISampleProvider input, string id, AudioDomain domain);
        void Dispose();
        DictionaryMixingSampleProvider GetMixer(AudioDomain domain);
        CachedAudio Play(CachedAudio audio, AudioDomain domain = AudioDomain.SFX);
        void RemoveMixerInput(string id, AudioDomain domain);
        void SetPlayList(Playlist newPlaylist, bool fade = true);
        void SetTrack(string id, bool fade = true);
        bool TryRemoveMixerInput(string id, AudioDomain domain);
        void Update(int elapsedMilliseconds);
    }
}