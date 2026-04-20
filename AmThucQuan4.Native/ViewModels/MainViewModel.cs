using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AmThucQuan4.Native.Models;
using AmThucQuan4.Native.Services;

namespace AmThucQuan4.Native.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IPoiService     _poiSvc;
    private readonly IMapService     _mapSvc;
    private readonly IAudioService   _audioSvc;
    private readonly ILanguageService _langSvc;

    // ── Collections ─────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<PoiModel> _pois = [];
    [ObservableProperty] private PoiModel? _selectedPoi;

    // ── User location — mặc định 800m bắc tuyến Đoàn Văn Bơ ────
    // Quận 4 center: ~10.758, 106.703 — cách POI xa nhất ~800m
    [ObservableProperty] private double _userLat  = 10.7610;
    [ObservableProperty] private double _userLng  = 106.7040;
    [ObservableProperty] private bool   _hasLocation;

    // ── Search ───────────────────────────────────────────────────
    [ObservableProperty] private string _searchText = "";

    // ── Route ────────────────────────────────────────────────────
    [ObservableProperty] private List<(double Lat, double Lng)> _routePoints = [];
    [ObservableProperty] private bool _hasRoute;

    // ── UI state ─────────────────────────────────────────────────
    [ObservableProperty] private bool _isAudioPlaying;
    [ObservableProperty] private bool _isDetailVisible;
    [ObservableProperty] private bool _isListExpanded = true;

    // ── Localised UI strings (bound to XAML) ─────────────────────
    [ObservableProperty] private string _uiNearbyPlaces  = "Địa điểm gần bạn";
    [ObservableProperty] private string _uiSearchHint    = "Tìm địa chỉ hoặc quán ăn...";
    [ObservableProperty] private string _uiNavigate      = "🗺️ Dẫn đường";
    [ObservableProperty] private string _uiPlayAudio     = "▶ Phát Audio";
    [ObservableProperty] private string _uiStopAudio     = "⏹ Dừng";
    [ObservableProperty] private string _uiRestaurantsCount = "quán";

    // ── Events → View ────────────────────────────────────────────
    public event Action<PoiModel>?                    FlyToPoiRequested;
    public event Action<List<(double, double)>>?      RouteUpdated;
    public event Action<double, double>?              UserLocationUpdated;
    public event Action?                              PoisRefreshed;  // ← trigger redraw map

    public MainViewModel(
        IPoiService     poiSvc,
        IMapService     mapSvc,
        IAudioService   audioSvc,
        ILanguageService langSvc,
        IApiService?    apiSvc = null)
    {
        _poiSvc   = poiSvc;
        _mapSvc   = mapSvc;
        _audioSvc = audioSvc;
        _langSvc  = langSvc;

        _audioSvc.PlaybackStopped += () =>
            MainThread.BeginInvokeOnMainThread(
                () => IsAudioPlaying = false);

        // Khi đổi ngôn ngữ → cập nhật tất cả UI strings
        _langSvc.LanguageChanged += RefreshUiStrings;
    }

    // ── Load POIs ────────────────────────────────────────────────
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusText = "Đang tải dữ liệu...";
        try
        {
            var list = await _poiSvc.GetAllAsync();
            Pois.Clear();
            foreach (var p in list) Pois.Add(p);
            StatusText = $"Tải xong {Pois.Count} địa điểm";

            // Trigger redraw pins — KHÔNG auto-set vị trí user
            PoisRefreshed?.Invoke();
        }
        finally { IsBusy = false; }
    }

    /// <summary>
    /// Gọi từ Admin sau khi thêm/sửa/xóa POI.
    /// Dùng PoiService (đã có HttpClient riêng) thay vì AdminService
    /// để tránh tạo thêm request thừa.
    /// </summary>
    public async Task RefreshPoisFromApiAsync(IAdminService? _ = null)
    {
        var list = await _poiSvc.GetAllAsync();
        Pois.Clear();
        foreach (var p in list) Pois.Add(p);

        if (HasLocation) UpdateDistances();

        // Báo MainPage redraw pins
        PoisRefreshed?.Invoke();
    }

    // ── GPS ──────────────────────────────────────────────────────
    [RelayCommand]
    public async Task GetLocationAsync()
    {
        StatusText = "Đang lấy vị trí GPS...";
        try
        {
            // Kiểm tra quyền
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                StatusText = "Chưa cấp quyền vị trí";
                return;
            }

            // Thử last known trước (nhanh hơn, ít crash hơn)
            var loc = await Geolocation.GetLastKnownLocationAsync();

            // Nếu không có → lấy GPS thật với timeout ngắn
            if (loc == null)
            {
                loc = await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Low,
                                           TimeSpan.FromSeconds(8)));
            }

            if (loc == null)
            {
                StatusText = "Không lấy được vị trí, thử lại";
                return;
            }

            UserLat     = loc.Latitude;
            UserLng     = loc.Longitude;
            HasLocation = true;
            StatusText  = "Đã cập nhật vị trí ✓";

            UpdateDistances();
            UserLocationUpdated?.Invoke(UserLat, UserLng);
        }
        catch (FeatureNotSupportedException)
        {
            StatusText = "Thiết bị không hỗ trợ GPS";
        }
        catch (PermissionException)
        {
            StatusText = "Chưa cấp quyền GPS";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GPS] {ex}");
            StatusText = "Lỗi GPS, dùng vị trí mặc định";
            // Fallback: dùng tọa độ mặc định Quận 4
            UserLat     = 10.7610;
            UserLng     = 106.7040;
            HasLocation = true;
            UpdateDistances();
            UserLocationUpdated?.Invoke(UserLat, UserLng);
        }
    }

    // ── Chọn POI ─────────────────────────────────────────────────
    [RelayCommand]
    public async Task SelectPoiAsync(PoiModel poi)
    {
        SelectedPoi       = poi;
        IsDetailVisible   = true;
        FlyToPoiRequested?.Invoke(poi);
        // KHÔNG tự vẽ đường — user phải bấm nút "Dẫn đường" chủ động
    }

    // ── Vẽ đường — chỉ chạy khi user đã đặt vị trí ─────────────
    [RelayCommand]
    public async Task DrawRouteAsync(PoiModel poi)
    {
        if (!HasLocation)
        {
            StatusText = "⚠️ Hãy bấm 📌 để chọn vị trí của bạn trước!";
            return;
        }

        IsBusy     = true;
        StatusText = $"Đang tính đường đến {poi.Name}...";
        try
        {
            var pts = await _mapSvc.GetRouteAsync(
                UserLat, UserLng,
                poi.Latitude, poi.Longitude);

            RoutePoints = pts;
            HasRoute    = pts.Count > 0;
            RouteUpdated?.Invoke(pts);
            StatusText  = HasRoute
                ? $"Đường đến {poi.Name} ✓"
                : $"Không tìm được đường";
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi dẫn đường: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    // ── Tìm địa chỉ ──────────────────────────────────────────────
    [RelayCommand]
    public async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;
        IsBusy     = true;
        StatusText = $"Đang tìm \"{SearchText}\"...";
        try
        {
            var r = await _mapSvc.GeocodeAsync(SearchText);
            if (r == null) { StatusText = "Không tìm thấy địa chỉ"; return; }

            UserLat     = r.Value.Lat;
            UserLng     = r.Value.Lng;
            HasLocation = true;
            UpdateDistances();
            UserLocationUpdated?.Invoke(UserLat, UserLng);

            // Fly map đến vị trí tìm được
            FlyToPoiRequested?.Invoke(new PoiModel
            {
                Name      = SearchText,
                Latitude  = r.Value.Lat,
                Longitude = r.Value.Lng,
            });
            StatusText = $"Tìm thấy: {SearchText}";
        }
        finally { IsBusy = false; }
    }

    // ── Toggle Audio ─────────────────────────────────────────────
    [RelayCommand]
    public async Task ToggleAudioAsync()
    {
        if (SelectedPoi == null) return;

        if (IsAudioPlaying)
        {
            _audioSvc.Stop();
            IsAudioPlaying = false;
            return;
        }

        IsAudioPlaying = true;

        // Lấy script theo ngôn ngữ hiện tại
        var script = _langSvc.GetAudioScript(
            SelectedPoi.Id, SelectedPoi.AudioScript);

        if (string.IsNullOrWhiteSpace(script))
            script = $"{SelectedPoi.Name}. {SelectedPoi.Description}";

        // Phát TTS với locale của ngôn ngữ được chọn
        await _audioSvc.PlayTtsAsync(
            script, SelectedPoi.Id, _langSvc.TtsLocale);

        IsAudioPlaying = false;
    }

    // ── Refresh toàn bộ UI strings khi đổi ngôn ngữ ─────────────
    private void RefreshUiStrings()
    {
        UiNearbyPlaces     = _langSvc.Ui("nearby_places");
        UiSearchHint       = _langSvc.Ui("search_placeholder");
        UiNavigate         = _langSvc.Ui("navigate");
        UiPlayAudio        = _langSvc.Ui("play_audio");
        UiStopAudio        = _langSvc.Ui("stop_audio");
        UiRestaurantsCount = _langSvc.Ui("restaurants_count");

        // Cập nhật nội dung POI theo ngôn ngữ mới
        foreach (var poi in Pois)
        {
            poi.Name        = _langSvc.GetName(poi.Id, poi.Name);
            poi.Description = _langSvc.GetDescription(poi.Id, poi.Description);
            poi.Address     = _langSvc.GetAddress(poi.Id, poi.Address);
        }

        // Trigger selected POI refresh nếu đang mở detail
        if (SelectedPoi != null)
        {
            var updated = Pois.FirstOrDefault(p =>
                p.Id == SelectedPoi.Id);
            if (updated != null) SelectedPoi = updated;
        }
    }

    // ── Xóa route ────────────────────────────────────────────────
    [RelayCommand]
    public void ClearRoute()
    {
        RoutePoints = [];
        HasRoute    = false;
        RouteUpdated?.Invoke([]);
    }

    // ── Đóng detail panel ────────────────────────────────────────
    [RelayCommand]
    public void CloseDetail()
    {
        SelectedPoi     = null;
        IsDetailVisible = false;
        _audioSvc.Stop();
        IsAudioPlaying = false;
        ClearRoute();
    }

    // ── Helper: Haversine ────────────────────────────────────────
    // Public alias dùng từ MainPage khi user tự chọn vị trí trên map
    public void UpdateDistancesPublic() => UpdateDistances();

    private void UpdateDistances()
    {
        foreach (var p in Pois)
        {
            // Setter tự trigger NotifyPropertyChangedFor(DistanceText)
            p.DistanceMeters = Haversine(UserLat, UserLng, p.Latitude, p.Longitude);
        }
        var sorted = Pois.OrderBy(p => p.DistanceMeters).ToList();
        Pois.Clear();
        foreach (var p in sorted) Pois.Add(p);
    }

    private static double Haversine(
        double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6_371_000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLng = (lng2 - lng1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
