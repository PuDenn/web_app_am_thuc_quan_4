using Plugin.Maui.Audio;

namespace AmThucQuan4.Native.Services;

public class AudioService : IAudioService, IDisposable
{
    private readonly IAudioManager _mgr;
    private IAudioPlayer? _player;
    private CancellationTokenSource? _ttsCts;

    public bool    IsPlaying    => _player?.IsPlaying ?? false;
    public string? CurrentPoiId { get; private set; }
    public event Action? PlaybackStopped;

    public AudioService(IAudioManager mgr) => _mgr = mgr;

    // ─── Phát file .mp3 (nếu có trong app bundle) ───────────────
    public async Task PlayFileAsync(string audioPath, string poiId)
    {
        Stop();
        try
        {
            var stream  = await FileSystem.OpenAppPackageFileAsync(audioPath);
            _player     = _mgr.CreatePlayer(stream);
            CurrentPoiId = poiId;
            _player.PlaybackEnded += (_, _) =>
            {
                CurrentPoiId = null;
                MainThread.BeginInvokeOnMainThread(
                    () => PlaybackStopped?.Invoke());
            };
            _player.Volume = 1.0;
            _player.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioFile] {ex.Message}");
            // File không tồn tại → không làm gì, caller tự fallback TTS
            CurrentPoiId = null;
            PlaybackStopped?.Invoke();
        }
    }

    // ─── Phát TTS với script đầy đủ ─────────────────────────────
    // Cache danh sách locale — GetLocalesAsync chậm, chỉ gọi 1 lần
    private IEnumerable<Locale>? _cachedLocales;

    public async Task PlayTtsAsync(string script, string poiId, string? locale = null)
    {
        Stop();
        _ttsCts      = new CancellationTokenSource();
        CurrentPoiId = poiId;

        try
        {
            // Dùng cache thay vì gọi GetLocalesAsync mỗi lần
            _cachedLocales ??= await TextToSpeech.GetLocalesAsync();
            var locales    = _cachedLocales;
            var targetLang = locale?.Split('-')[0] ?? "vi";

            var selectedLocale = locales.FirstOrDefault(l =>
                l.Language.StartsWith(targetLang,
                    StringComparison.OrdinalIgnoreCase))
                ?? locales.FirstOrDefault(l =>
                    l.Language.StartsWith("en",
                        StringComparison.OrdinalIgnoreCase));

            System.Diagnostics.Debug.WriteLine(
                $"[TTS] Lang={targetLang} | {script[..Math.Min(50, script.Length)]}...");

            await TextToSpeech.SpeakAsync(script, new SpeechOptions
            {
                Locale = selectedLocale,
                Volume = 1.0f,
                Pitch  = 1.0f,
            }, _ttsCts.Token);
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("[TTS] Cancelled");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TTS] {ex.Message}");
        }
        finally
        {
            CurrentPoiId = null;
            MainThread.BeginInvokeOnMainThread(
                () => PlaybackStopped?.Invoke());
        }
    }

    public void Stop()
    {
        // Dừng file player
        if (_player != null)
        {
            _player.Stop();
            _player.Dispose();
            _player = null;
        }

        // Dừng TTS
        _ttsCts?.Cancel();
        _ttsCts?.Dispose();
        _ttsCts = null;

        // Cancel TTS qua CancellationToken (đã xử lý ở trên)
        // TextToSpeech không có StopSpeaking API — dùng CancellationToken để cancel

        CurrentPoiId = null;
    }

    public void Dispose() => Stop();
}
