using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AmThucQuan4.Native.Models;

namespace AmThucQuan4.Native.Services;

public class AdminService : IAdminService
{
    private readonly HttpClient  _http;
    private readonly IApiService _api;

    private const string BaseUrl = "http://10.0.2.2:5000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public AdminService(HttpClient http, IApiService api)
    {
        _http         = http;
        _api          = api;
        _http.BaseAddress = new Uri(BaseUrl);
        _http.Timeout     = TimeSpan.FromSeconds(10);
    }

    // ─── Gắn JWT token trước mỗi request ────────────────────────
    private void SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(_api.Token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _api.Token);
    }

    // ══════════════════════════════════════════
    // POI CRUD
    // ══════════════════════════════════════════

    public async Task<List<PoiModel>> GetAllPoisAsync()
    {
        try
        {
            SetAuthHeader();
            var pois = await _http.GetFromJsonAsync<List<ApiPoi>>(
                "/api/pois", JsonOpts);

            return pois?.Select(MapToPoi).ToList() ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin GetPois] {ex.Message}");
            return [];
        }
    }

    public async Task<bool> CreatePoiAsync(PoiModel poi)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.PostAsJsonAsync("/api/pois", MapToApi(poi));
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin CreatePoi] {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdatePoiAsync(PoiModel poi)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.PutAsJsonAsync(
                $"/api/pois/{poi.Id}", MapToApi(poi));
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin UpdatePoi] {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeletePoiAsync(string id)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.DeleteAsync($"/api/pois/{id}");
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin DeletePoi] {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TogglePoiVisibilityAsync(string id)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.PatchAsync(
                $"/api/pois/{id}/toggle", null);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin TogglePoi] {ex.Message}");
            return false;
        }
    }

    // ══════════════════════════════════════════
    // USER MANAGEMENT
    // ══════════════════════════════════════════

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        try
        {
            SetAuthHeader();
            var users = await _http.GetFromJsonAsync<List<UserModel>>(
                "/api/users", JsonOpts);
            return users ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin GetUsers] {ex.Message}");
            return [];
        }
    }

    // ─── Admin tạo user mới — KHÔNG ghi đè token hiện tại ───────
    public async Task<(bool Success, string Message)> CreateUserAsync(
        string username, string email, string password, string role)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.PostAsJsonAsync("/api/users/create",
                new { username, email, password, role });

            var body = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
                return (true, $"Đã tạo tài khoản {username}");

            // Parse error message từ API
            try
            {
                var err = System.Text.Json.JsonSerializer
                    .Deserialize<System.Text.Json.JsonElement>(body);
                var msg = err.GetProperty("message").GetString() ?? "Lỗi không xác định";
                return (false, msg);
            }
            catch { return (false, "Tạo tài khoản thất bại"); }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin CreateUser] {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<bool> ToggleUserActiveAsync(int id)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.PatchAsync(
                $"/api/users/{id}/toggle", null);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin ToggleUser] {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            SetAuthHeader();
            var res = await _http.DeleteAsync($"/api/users/{id}");
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Admin DeleteUser] {ex.Message}");
            return false;
        }
    }

    // ─── Mapper helpers ──────────────────────────────────────────
    private static PoiModel MapToPoi(ApiPoi p) => new()
    {
        Id           = p.Id.ToString(),
        Name         = p.Name,
        Category     = p.Category,
        CategoryIcon = p.CategoryIcon,
        Description  = p.Description,
        Address      = p.Address,
        Hours        = p.Hours,
        PriceRange   = p.PriceRange,
        ImageUrl     = p.ImageUrl ?? "",
        AudioScript  = p.AudioScript ?? "",
        Latitude     = p.Latitude,
        Longitude    = p.Longitude,
    };

    private static object MapToApi(PoiModel p) => new
    {
        Name         = p.Name,
        Category     = p.Category,
        CategoryIcon = p.CategoryIcon,
        Description  = p.Description,
        Address      = p.Address,
        Hours        = p.Hours,
        PriceRange   = p.PriceRange,
        ImageUrl     = p.ImageUrl,
        AudioScript  = p.AudioScript,
        Latitude     = p.Latitude,
        Longitude    = p.Longitude,
        IsVisible    = true,
    };

    private record ApiPoi(
        int     Id,
        string  Name,
        string  Category,
        string  CategoryIcon,
        string  Description,
        string  Address,
        string  Hours,
        string  PriceRange,
        string? ImageUrl,
        string? AudioPath,
        string? AudioScript,
        double  Latitude,
        double  Longitude,
        bool    IsVisible);
}
