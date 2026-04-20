namespace AmThucQuan4.Native.Services;

public interface IAudioService
{
    bool    IsPlaying    { get; }
    string? CurrentPoiId { get; }
    event Action? PlaybackStopped;

    /// <summary>Phát file .mp3 từ app bundle</summary>
    Task PlayFileAsync(string audioPath, string poiId);

    /// <summary>Phát TTS với nội dung script đầy đủ theo locale chỉ định</summary>
    Task PlayTtsAsync(string script, string poiId, string? locale = null);

    void Stop();
}
