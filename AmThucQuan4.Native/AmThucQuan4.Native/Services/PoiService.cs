using System.Net.Http.Json;
using System.Text.Json;
using AmThucQuan4.Native.Models;

namespace AmThucQuan4.Native.Services;

/// <summary>
/// Lấy POI từ API thay vì hardcode — để Admin thêm/sửa/xóa hiển thị ngay.
/// </summary>
public class PoiService : IPoiService
{
    private readonly HttpClient  _http;
    private readonly IApiService _api;
    private const string BaseUrl = "http://10.0.2.2:5000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public PoiService(HttpClient http, IApiService api)
    {
        _http             = http;
        _api              = api;
        _http.BaseAddress = new Uri(BaseUrl);
        _http.Timeout     = TimeSpan.FromSeconds(10);
    }

    public async Task<List<PoiModel>> GetAllAsync()
    {
        try
        {
            // Gắn token nếu có
            if (!string.IsNullOrEmpty(_api.Token))
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", _api.Token);

            var pois = await _http.GetFromJsonAsync<List<ApiPoi>>(
                "/api/pois", JsonOpts);

            if (pois == null || pois.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[PoiService] API trả về rỗng, dùng fallback data");
                return GetFallbackData();
            }

            System.Diagnostics.Debug.WriteLine($"[PoiService] Loaded {pois.Count} POIs from API");
            return pois.Select(MapToPoi).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PoiService] API lỗi: {ex.Message} — dùng fallback");
            return GetFallbackData();
        }
    }

    public async Task<PoiModel?> GetByIdAsync(string id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(p => p.Id == id);
    }

    // ─── Mapper ─────────────────────────────────────────────────
    private static PoiModel MapToPoi(ApiPoi p) => new()
    {
        Id           = p.Id.ToString(),
        Name         = p.Name,
        Category     = p.Category     ?? "Ẩm thực",
        CategoryIcon = p.CategoryIcon ?? "🍽️",
        Description  = p.Description  ?? "",
        Address      = p.Address      ?? "",
        Hours        = p.Hours        ?? "",
        PriceRange   = p.PriceRange   ?? "",
        ImageUrl     = p.ImageUrl     ?? "",
        AudioScript  = p.AudioScript  ?? p.Name,
        Latitude     = p.Latitude,
        Longitude    = p.Longitude,
    };

    private record ApiPoi(
        int     Id,
        string  Name,
        string? Category,
        string? CategoryIcon,
        string? Description,
        string? Address,
        string? Hours,
        string? PriceRange,
        string? ImageUrl,
        string? AudioPath,
        string? AudioScript,
        double  Latitude,
        double  Longitude,
        bool    IsVisible);

    // ─── Fallback nếu API chưa chạy / offline ───────────────────
    private static List<PoiModel> GetFallbackData() =>
    [
        new() {
            Id = "poi-1", Name = "Cơm Tấm Bà Út",
            Category = "Cơm Tấm", CategoryIcon = "🍚",
            Description = "Cơm tấm sườn nướng than hoa chuẩn vị Sài Gòn, nước mắm pha gia truyền.",
            Address = "32 Hoàng Diệu, Phường 10, Quận 4",
            Hours = "06:00 – 14:00", PriceRange = "35k – 65k",
            ImageUrl = "https://images.unsplash.com/photo-1630984931587-5db2946e5c40?w=600&q=80",
            AudioScript = "Chào mừng bạn đến Cơm Tấm Bà Út tại 32 Hoàng Diệu. Sườn nướng than hoa thơm lừng, nước mắm pha gia truyền 30 năm. Giá từ 35 đến 65 nghìn.",
            Latitude = 10.7573, Longitude = 106.7000,
        },
        new() {
            Id = "poi-2", Name = "Bánh Mì Huỳnh Hoa",
            Category = "Bánh Mì", CategoryIcon = "🥖",
            Description = "Bánh mì giòn rụm nhân đầy chả lụa, thịt nguội, pa-tê nổi tiếng Sài Gòn.",
            Address = "26 Đoàn Văn Bơ, Phường 13, Quận 4",
            Hours = "06:00 – 14:00", PriceRange = "30k – 45k",
            ImageUrl = "https://images.unsplash.com/photo-1509722747041-616f39b57569?w=600&q=80",
            AudioScript = "Bánh Mì Huỳnh Hoa tại 26 Đoàn Văn Bơ. Ổ bánh giòn rụm, nhân đầy pa-tê béo ngậy. Giá từ 30 đến 45 nghìn.",
            Latitude = 10.7575, Longitude = 106.7012,
        },
        new() {
            Id = "poi-3", Name = "Ốc Đào",
            Category = "Ốc", CategoryIcon = "🐚",
            Description = "Ốc len xào dừa, nghêu hấp sả thơm lừng đặc trưng Sài Gòn.",
            Address = "5 Đoàn Văn Bơ, Phường 12, Quận 4",
            Hours = "17:00 – 23:00", PriceRange = "50k – 150k",
            ImageUrl = "https://images.unsplash.com/photo-1559410545-0bdcd187e0a6?w=600&q=80",
            AudioScript = "Ốc Đào tại 5 Đoàn Văn Bơ. Ốc len xào dừa béo ngậy, nghêu hấp sả thơm lừng. Mở từ 5 giờ chiều.",
            Latitude = 10.7577, Longitude = 106.7021,
        },
        new() {
            Id = "poi-4", Name = "Trà Sữa Phúc Long",
            Category = "Trà Sữa", CategoryIcon = "🧋",
            Description = "Trà ô long sữa và cà phê phin truyền thống thương hiệu Việt 60 năm.",
            Address = "10 Đoàn Văn Bơ, Phường 12, Quận 4",
            Hours = "07:00 – 22:00", PriceRange = "29k – 65k",
            ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&q=80",
            AudioScript = "Phúc Long tại 10 Đoàn Văn Bơ. Trà ô long sữa thơm đặc trưng, cà phê phin đậm đà. Giá từ 29 đến 65 nghìn.",
            Latitude = 10.7580, Longitude = 106.7033,
        },
        new() {
            Id = "poi-5", Name = "Phở 24",
            Category = "Phở", CategoryIcon = "🍜",
            Description = "Nước dùng trong vắt ninh xương bò tươi 8 tiếng, thịt bò tái mềm tan.",
            Address = "8 Nguyễn Tất Thành, Phường 13, Quận 4",
            Hours = "06:00 – 10:30", PriceRange = "45k – 75k",
            ImageUrl = "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?w=600&q=80",
            AudioScript = "Phở 24 tại 8 Nguyễn Tất Thành. Nước dùng ninh 8 tiếng trong vắt ngọt thanh. Chỉ mở đến 10 giờ rưỡi sáng.",
            Latitude = 10.7583, Longitude = 106.7048,
        },
    ];
}
