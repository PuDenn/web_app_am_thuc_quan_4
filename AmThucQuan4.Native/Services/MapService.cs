using System.Net.Http.Json;
using System.Text.Json;

namespace AmThucQuan4.Native.Services;

public class MapService : IMapService
{
    private readonly HttpClient _http;

    private const string OsrmBase      = "https://router.project-osrm.org/route/v1/foot";
    private const string NominatimBase = "https://nominatim.openstreetmap.org/search";

    public MapService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("AmThucQuan4Native/1.0");
        _http.Timeout = TimeSpan.FromSeconds(10);
    }

    // ─── OSRM: routing walking ───────────────────────────────────
    public async Task<List<(double Lat, double Lng)>> GetRouteAsync(
        double fromLat, double fromLng,
        double toLat,   double toLng)
    {
        var url = $"{OsrmBase}/{fromLng},{fromLat};{toLng},{toLat}" +
                  "?overview=full&geometries=geojson";
        try
        {
            var json = await _http.GetFromJsonAsync<JsonElement>(url);
            var coords = json
                .GetProperty("routes")[0]
                .GetProperty("geometry")
                .GetProperty("coordinates");

            return coords.EnumerateArray()
                .Select(c => (c[1].GetDouble(), c[0].GetDouble()))
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Route] {ex.Message}");
            // Fallback: đường thẳng
            return [(fromLat, fromLng), (toLat, toLng)];
        }
    }

    // ─── Nominatim: geocoding ────────────────────────────────────
    public async Task<(double Lat, double Lng)?> GeocodeAsync(string address)
    {
        var q = Uri.EscapeDataString(
            address.Contains("Quận") ? address : address + " Quận 4 TP.HCM");
        var url = $"{NominatimBase}?q={q}&format=json&limit=1&countrycodes=vn";
        try
        {
            var json = await _http.GetFromJsonAsync<JsonElement[]>(url);
            if (json == null || json.Length == 0) return null;

            var lat = double.Parse(
                json[0].GetProperty("lat").GetString()!,
                System.Globalization.CultureInfo.InvariantCulture);
            var lng = double.Parse(
                json[0].GetProperty("lon").GetString()!,
                System.Globalization.CultureInfo.InvariantCulture);
            return (lat, lng);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Geocode] {ex.Message}");
            return null;
        }
    }
}
