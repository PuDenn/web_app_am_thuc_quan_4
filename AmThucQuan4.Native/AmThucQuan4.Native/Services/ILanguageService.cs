namespace AmThucQuan4.Native.Services;

public interface ILanguageService
{
    string CurrentCode { get; }
    string CurrentName { get; }
    string CurrentFlag { get; }
    string? TtsLocale  { get; }

    List<LanguageOption> Available { get; }
    void SetLanguage(string code);

    // Nội dung POI theo ngôn ngữ
    string GetName(string poiId, string fallback);
    string GetDescription(string poiId, string fallback);
    string GetAudioScript(string poiId, string fallback);
    string GetAddress(string poiId, string fallback);

    // UI strings theo ngôn ngữ
    string Ui(string key);

    event Action? LanguageChanged;
}

public record LanguageOption(
    string Code,
    string Name,
    string Flag,
    string? TtsLocale);
