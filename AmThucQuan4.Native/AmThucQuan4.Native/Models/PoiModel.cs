using CommunityToolkit.Mvvm.ComponentModel;

namespace AmThucQuan4.Native.Models;

public partial class PoiModel : ObservableObject
{
    public string Id           { get; set; } = "";
    public string Name         { get; set; } = "";
    public string Category     { get; set; } = "";
    public string Description  { get; set; } = "";
    public string Address      { get; set; } = "";
    public string Hours        { get; set; } = "";
    public string PriceRange   { get; set; } = "";
    public string AudioPath    { get; set; } = "";
    public string CategoryIcon { get; set; } = "🍽️";
    public string ImageUrl     { get; set; } = "";  // ← ảnh món ăn
    public string AudioScript  { get; set; } = "";  // ← toàn bộ nội dung TTS
    public double Latitude     { get; set; }
    public double Longitude    { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DistanceText))]
    private double _distanceMeters;

    public string DistanceText =>
        DistanceMeters <= 0   ? "" :
        DistanceMeters < 1000 ? $"{DistanceMeters:F0}m" :
                                $"{DistanceMeters / 1000:F1}km";
}
