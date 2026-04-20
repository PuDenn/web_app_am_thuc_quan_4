// Alias tránh conflict namespace Mapsui vs MAUI
using AmThucQuan4.Native.Services;
using MColor  = Mapsui.Styles.Color;
using MBrush  = Mapsui.Styles.Brush;
using MFont   = Mapsui.Styles.Font;
using MPen    = Mapsui.Styles.Pen;
using MOffset = Mapsui.Styles.Offset;

using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using AmThucQuan4.Native.Models;
using AmThucQuan4.Native.ViewModels;

namespace AmThucQuan4.Native.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel  _vm;
    private readonly IApiService    _api;
    private readonly ILanguageService _lang;
    private MemoryLayer? _poiLayer;
    private MemoryLayer? _userLayer;
    private MemoryLayer? _routeLayer;
    private bool _sheetExpanded = false;

    private const double Zoom15 = 4.7775;
    private const double Zoom16 = 2.3887;
    private const double Zoom17 = 1.1944;

    private static MPoint ToMPoint(double lon, double lat)
    {
        var (x, y) = SphericalMercator.FromLonLat(lon, lat);
        return new MPoint(x, y);
    }

    // Trung tâm tuyến Đoàn Văn Bơ — 5 POI nằm trong bán kính ~300m
    private static readonly MPoint Q4Center = ToMPoint(106.7024, 10.7578);

    public MainPage(MainViewModel vm, IApiService api, ILanguageService lang)
    {
        InitializeComponent();
        BindingContext = _vm  = vm;
        _api           = api;
        _lang          = lang;

        // Lắng nghe thay đổi ngôn ngữ → cập nhật UI (debounced)
        _lang.LanguageChanged += () =>
            MainThread.BeginInvokeOnMainThread(() =>
            {
                btnLang.Text = _lang.CurrentFlag;
                UpdateAudioBtn();
                // Redraw pins sau 200ms để tránh lag khi đổi ngôn ngữ
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200),
                    DrawPoiPins);
            });

        Loaded += (_, _) =>
        {
            btnAdmin.IsVisible = _api.Role == "Admin";
            btnLang.Text       = _lang.CurrentFlag;
            UpdateAudioBtn();
        };

        // Cập nhật nút audio khi IsAudioPlaying thay đổi
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_vm.IsAudioPlaying))
                MainThread.BeginInvokeOnMainThread(UpdateAudioBtn);
        };

        _vm.FlyToPoiRequested   += OnFlyToPoi;
        _vm.RouteUpdated        += OnRouteUpdated;
        _vm.UserLocationUpdated += OnUserLocationUpdated;
        _vm.PoisRefreshed += () =>
            MainThread.BeginInvokeOnMainThread(() =>
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100),
                    DrawPoiPins));

        // Khởi động map khi page loaded
        Loaded += OnPageLoaded;
    }

    // ─── Page fully loaded → init map ───────────────────────────
    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        try
        {
            InitMap();
            await _vm.LoadAsync();
            // DrawPoiPins đã được gọi qua PoisRefreshed event trong LoadAsync
            // Không cần gọi thêm ở đây tránh double-redraw
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PageLoaded] {ex}");
        }
    }

    // ─── Init Mapsui (performance optimized) ───────────────────
    private void InitMap()
    {
        var map = new Mapsui.Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        _routeLayer = new MemoryLayer { Name = "Route" };
        _poiLayer   = new MemoryLayer { Name = "POIs"  };
        _userLayer  = new MemoryLayer { Name = "User"  };

        map.Layers.Add(_routeLayer);
        map.Layers.Add(_poiLayer);
        map.Layers.Add(_userLayer);

        map.BackColor = Mapsui.Styles.Color.FromString("#0f172a");

        mapControl.Map   = map;
        mapControl.Info += OnMapTapped;

        map.Navigator.CenterOnAndZoomTo(Q4Center, Zoom15);
    }

    // ─── Vẽ POI pins ────────────────────────────────────────────
    private void DrawPoiPins()
    {
        if (_poiLayer == null) return;

        // Tạo features một lần, reuse static style để tránh tạo object mới mỗi frame
        var features = new List<IFeature>(_vm.Pois.Count);
        foreach (var poi in _vm.Pois)
        {
            try
            {
                var pt = ToMPoint(poi.Longitude, poi.Latitude);
                var f  = new PointFeature(pt);
                f["id"]   = poi.Id;
                f["name"] = poi.Name;

                // Pin tròn cam
                f.Styles.Add(new SymbolStyle
                {
                    SymbolScale = 0.65,
                    Fill        = new MBrush(new MColor(249, 115, 22)),
                    Outline     = new MPen(MColor.White, 2),
                });
                // Label nhỏ gọn
                f.Styles.Add(new LabelStyle
                {
                    Text      = poi.CategoryIcon + " " + poi.Name,
                    ForeColor = MColor.White,
                    BackColor = new MBrush(new MColor(15, 23, 42, 210)),
                    Font      = new MFont { Size = 10, Bold = false },
                    Offset    = new MOffset(0, -26),
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    MaxWidth  = 100,
                });
                features.Add(f);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Pin] {poi.Name}: {ex.Message}");
            }
        }

        _poiLayer.Features = features;
        _poiLayer.DataHasChanged();
    }

    // ─── User location marker ────────────────────────────────────
    private void OnUserLocationUpdated(double lat, double lng)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (_userLayer == null) return;

                var pt = ToMPoint(lng, lat);
                var f  = new PointFeature(pt);
                f.Styles.Add(new SymbolStyle
                {
                    SymbolScale = 0.6,
                    Fill        = new MBrush(new MColor(34, 197, 94)),
                    Outline     = new MPen(MColor.White, 3),
                });
                f.Styles.Add(new LabelStyle
                {
                    Text      = "Bạn",
                    ForeColor = MColor.White,
                    Font      = new MFont { Size = 11 },
                    Offset    = new MOffset(0, -22),
                });

                _userLayer.Features = new List<IFeature> { f };
                _userLayer.DataHasChanged();

                mapControl.Map?.Navigator.CenterOnAndZoomTo(pt, Zoom16, 500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserMarker] {ex.Message}");
            }
        });
    }

    // ─── Fly đến POI ────────────────────────────────────────────
    private void OnFlyToPoi(PoiModel poi)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var pt = ToMPoint(poi.Longitude, poi.Latitude);
                mapControl.Map?.Navigator.CenterOnAndZoomTo(pt, Zoom17, 600);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FlyTo] {ex.Message}");
            }
        });
    }

    // ─── Vẽ route polyline ──────────────────────────────────────
    private void OnRouteUpdated(List<(double Lat, double Lng)> pts)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (_routeLayer == null) return;

                if (pts.Count == 0)
                {
                    _routeLayer.Features = new List<IFeature>();
                    _routeLayer.DataHasChanged();
                    return;
                }

                var coords = pts.Select(p =>
                {
                    var (x, y) = SphericalMercator.FromLonLat(p.Lng, p.Lat);
                    return new Coordinate(x, y);
                }).ToArray();

                var line = new LineString(coords);
                var f    = new GeometryFeature { Geometry = line };
                f.Styles.Add(new VectorStyle
                {
                    Line = new MPen(new MColor(249, 115, 22), 5),
                });

                _routeLayer.Features = new List<IFeature> { f };
                _routeLayer.DataHasChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Route] {ex.Message}");
            }
        });
    }

    // ─── Chế độ chọn vị trí user ────────────────────────────────
    private bool _isPickingLocation = false;

    private void OnPickLocationModeClicked(object? sender, EventArgs e)
    {
        _isPickingLocation = !_isPickingLocation;

        if (_isPickingLocation)
        {
            // Đang chờ user chạm map
            btnPickLocation.BackgroundColor =
                Microsoft.Maui.Graphics.Color.FromArgb("#0891b2"); // xanh đậm hơn
            btnPickLocation.Text = "✋"; // biểu tượng chờ chạm

            _vm.StatusText = "Chạm vào bản đồ để đặt vị trí của bạn...";
        }
        else
        {
            btnPickLocation.BackgroundColor =
                Microsoft.Maui.Graphics.Color.FromArgb("#0e7490");
            btnPickLocation.Text = "📌";
            _vm.StatusText = "";
        }
    }

    // ─── Map click ──────────────────────────────────────────────
    private void OnMapTapped(object? sender, MapInfoEventArgs e)
    {
        // Chế độ chọn vị trí user
        if (_isPickingLocation)
        {
            var mapInfo = e.MapInfo;
            if (mapInfo?.WorldPosition == null) return;

            var (lon, lat) = SphericalMercator.ToLonLat(
                mapInfo.WorldPosition.X,
                mapInfo.WorldPosition.Y);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _vm.UserLat     = lat;
                _vm.UserLng     = lon;
                _vm.HasLocation = true;

                // Vẽ marker vị trí user
                OnUserLocationUpdated(lat, lon);

                // Tính lại khoảng cách tới từng POI
                _vm.UpdateDistancesPublic();

                // Thoát chế độ chọn vị trí
                _isPickingLocation = false;
                btnPickLocation.BackgroundColor =
                    Microsoft.Maui.Graphics.Color.FromArgb("#0e7490");
                btnPickLocation.Text = "📌";

                _vm.StatusText = "✅ Đã đặt vị trí — bấm 🗺️ Dẫn đường để vẽ đường";
            });
            return;
        }

        // Chế độ bình thường — chọn POI
        var poiId = e.MapInfo?.Feature?["id"] as string;
        if (string.IsNullOrEmpty(poiId)) return;

        var poi = _vm.Pois.FirstOrDefault(p => p.Id == poiId);
        if (poi == null) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try { await _vm.SelectPoiAsync(poi); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MapTap] {ex.Message}");
            }
        });
    }

    // ─── Cập nhật nút Audio theo ngôn ngữ + trạng thái ─────────
    private void UpdateAudioBtn()
    {
        if (btnAudio == null) return;
        btnAudio.Text = _vm.IsAudioPlaying
            ? _lang.Ui("stop_audio")
            : _lang.Ui("play_audio");
    }

    // ─── Chọn ngôn ngữ ──────────────────────────────────────────
    private async void OnLanguageClicked(object? sender, EventArgs e)
    {
        var options = _lang.Available
            .Select(l => $"{l.Flag} {l.Name}")
            .ToArray();

        var current = $"{_lang.CurrentFlag} {_lang.CurrentName}";

        var choice = await DisplayActionSheet(
            "🌐 Chọn ngôn ngữ thuyết minh",
            "Hủy",
            null,
            options);

        if (string.IsNullOrEmpty(choice) || choice == "Hủy") return;

        var selected = _lang.Available
            .FirstOrDefault(l => choice.Contains(l.Name));

        if (selected != null)
        {
            _lang.SetLanguage(selected.Code);
            await DisplayAlert("✅ Đã đổi ngôn ngữ",
                $"Thuyết minh sẽ bằng {selected.Flag} {selected.Name}", "OK");
        }
    }

    // ─── Đăng xuất ──────────────────────────────────────────────
    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Đăng xuất",
            "Bạn muốn đăng xuất khỏi tài khoản?",
            "Đăng xuất", "Hủy");

        if (!confirm) return;

        // Xóa token
        _api.Logout();

        // Reset ViewModel
        _vm.Pois.Clear();
        _vm.SelectedPoi    = null;
        _vm.IsDetailVisible = false;

        // Quay về LoginPage
        var loginPage = Handler?.MauiContext?.Services
            .GetRequiredService<Views.LoginPage>();
        if (loginPage != null && Window != null)
            Window.Page = loginPage;
    }

    // ─── Vào Admin Panel ────────────────────────────────────────
    private void OnAdminClicked(object? sender, EventArgs e)
    {
        var adminPage = Handler?.MauiContext?.Services
            .GetRequiredService<Views.Admin.AdminPage>();
        if (adminPage != null && Window != null)
            Window.Page = adminPage;
    }

    // ─── Bottom sheet toggle ────────────────────────────────────
    private void OnSheetHandleTapped(object? sender, TappedEventArgs e)
    {
        _sheetExpanded = !_sheetExpanded;
        var target = _sheetExpanded ? 420.0 : 300.0;

        var anim = new Microsoft.Maui.Controls.Animation(
            v => bottomSheet.HeightRequest = v,
            bottomSheet.HeightRequest,
            target,
            Easing.CubicOut);

        anim.Commit(this, "SheetAnim", 16, 250);
    }
}
