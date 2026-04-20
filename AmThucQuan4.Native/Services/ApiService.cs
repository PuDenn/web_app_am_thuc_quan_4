using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AmThucQuan4.Native.Models;

namespace AmThucQuan4.Native.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _http;

    // Android Emulator: 10.0.2.2 = localhost của máy tính host
    private const string BaseUrl = "http://10.0.2.2:5000";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public string? Token     { get; private set; }
    public string? Username  { get; private set; }
    public string? Role      { get; private set; }
    public bool    IsLoggedIn => !string.IsNullOrEmpty(Token);

    public ApiService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri(BaseUrl);
        _http.Timeout     = TimeSpan.FromSeconds(10);
    }

    // ─── Login ───────────────────────────────────────────────────
    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/auth/login",
                new { username, password });

            if (!res.IsSuccessStatusCode) return null;

            var result = await res.Content
                .ReadFromJsonAsync<LoginResult>(JsonOpts);

            if (result != null)
            {
                Token    = result.Token;
                Username = result.Username;
                Role     = result.Role;
                // Gắn token vào header cho các request tiếp theo
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Token);
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Login] {ex.Message}");
            return null;
        }
    }

    // ─── Register ────────────────────────────────────────────────
    public async Task<LoginResult?> RegisterAsync(
        string username, string email, string password)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/api/auth/register",
                new { username, email, password });

            if (!res.IsSuccessStatusCode) return null;

            var result = await res.Content
                .ReadFromJsonAsync<LoginResult>(JsonOpts);

            if (result != null)
            {
                Token    = result.Token;
                Username = result.Username;
                Role     = result.Role;
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Token);
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Register] {ex.Message}");
            return null;
        }
    }

    // ─── Lấy danh sách POI từ API ────────────────────────────────
    public async Task<List<PoiModel>> GetPoisAsync()
    {
        try
        {
            var pois = await _http.GetFromJsonAsync<List<ApiPoi>>(
                "/api/pois", JsonOpts);

            return pois?.Select(p => new PoiModel
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
                AudioPath    = p.AudioPath ?? "",
                AudioScript  = p.AudioScript ?? "",
                Latitude     = p.Latitude,
                Longitude    = p.Longitude,
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API GetPois] {ex.Message}");
            return [];
        }
    }

    public void Logout()
    {
        Token    = null;
        Username = null;
        Role     = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    // ─── DTO khớp với JSON trả về từ API ─────────────────────────
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
