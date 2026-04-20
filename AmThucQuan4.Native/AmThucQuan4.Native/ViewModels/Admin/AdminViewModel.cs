using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AmThucQuan4.Native.Models;
using AmThucQuan4.Native.Services;

namespace AmThucQuan4.Native.ViewModels.Admin;

public partial class AdminViewModel : ObservableObject
{
    private readonly IAdminService  _adminSvc;
    private readonly IApiService    _apiSvc;
    private MainViewModel?          _mainVm; // inject sau để tránh circular dep

    // ── Tab state ─────────────────────────────────────────────────
    [ObservableProperty] private bool _isPoisTab  = true;
    [ObservableProperty] private bool _isUsersTab = false;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _statusText = "";

    // ── POI data ─────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<PoiModel> _pois = [];
    [ObservableProperty] private PoiModel? _editingPoi;
    [ObservableProperty] private bool _isPoiFormVisible;
    [ObservableProperty] private bool _isEditMode;   // true=Edit, false=Add
    [ObservableProperty] private bool _hasPoiImage;  // preview ảnh

    partial void OnEditingPoiChanged(PoiModel? value)
        => HasPoiImage = !string.IsNullOrWhiteSpace(value?.ImageUrl);

    public void NotifyImageUrlChanged(string? url)
    {
        HasPoiImage = !string.IsNullOrWhiteSpace(url);
        if (EditingPoi != null) EditingPoi.ImageUrl = url ?? "";
    }

    // ── User data ─────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<UserModel> _users = [];

    // ── Admin info ────────────────────────────────────────────────
    public string AdminUsername => _apiSvc.Username ?? "Admin";

    // ── Events → View ─────────────────────────────────────────────
    public event Action<string>?  ShowAlert;
    public event Func<string, Task<bool>>? ConfirmDelete;

    public AdminViewModel(IAdminService adminSvc, IApiService apiSvc)
    {
        _adminSvc = adminSvc;
        _apiSvc   = apiSvc;
    }

    // Inject MainViewModel để trigger refresh map sau khi lưu POI
    public void SetMainViewModel(MainViewModel mainVm) => _mainVm = mainVm;

    // ── Load dữ liệu khi mở Admin ────────────────────────────────
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        StatusText = "Đang tải...";
        try
        {
            await LoadPoisAsync();
            await LoadUsersAsync();
            StatusText = "Đã tải xong";
        }
        finally { IsBusy = false; }
    }

    // ══════════════════════════════════════════
    // POI MANAGEMENT
    // ══════════════════════════════════════════

    [RelayCommand]
    public async Task LoadPoisAsync()
    {
        var list = await _adminSvc.GetAllPoisAsync();
        Pois.Clear();
        foreach (var p in list) Pois.Add(p);
    }

    // Mở form thêm POI mới
    [RelayCommand]
    public void OpenAddPoiForm()
    {
        EditingPoi = new PoiModel
        {
            CategoryIcon = "🍽️",
            Latitude     = 10.7575,
            Longitude    = 106.7012,
        };
        IsEditMode       = false;
        IsPoiFormVisible = true;
    }

    // Mở form sửa POI
    [RelayCommand]
    public void OpenEditPoiForm(PoiModel poi)
    {
        // Clone để không ảnh hưởng list gốc khi cancel
        EditingPoi = new PoiModel
        {
            Id           = poi.Id,
            Name         = poi.Name,
            Category     = poi.Category,
            CategoryIcon = poi.CategoryIcon,
            Description  = poi.Description,
            Address      = poi.Address,
            Hours        = poi.Hours,
            PriceRange   = poi.PriceRange,
            ImageUrl     = poi.ImageUrl,
            AudioScript  = poi.AudioScript,
            Latitude     = poi.Latitude,
            Longitude    = poi.Longitude,
        };
        IsEditMode       = true;
        IsPoiFormVisible = true;
    }

    // Lưu POI (thêm hoặc sửa)
    [RelayCommand]
    public async Task SavePoiAsync()
    {
        if (EditingPoi == null) return;

        if (string.IsNullOrWhiteSpace(EditingPoi.Name))
        {
            ShowAlert?.Invoke("Vui lòng nhập tên quán!");
            return;
        }

        IsBusy = true;
        try
        {
            bool ok;
            if (IsEditMode)
            {
                ok = await _adminSvc.UpdatePoiAsync(EditingPoi);
                StatusText = ok ? $"✅ Đã cập nhật {EditingPoi.Name}" : "❌ Cập nhật thất bại";
            }
            else
            {
                ok = await _adminSvc.CreatePoiAsync(EditingPoi);
                StatusText = ok ? $"✅ Đã thêm {EditingPoi.Name}" : "❌ Thêm thất bại";
            }

            if (ok)
            {
                IsPoiFormVisible = false;
                await LoadPoisAsync();

                // Refresh MainPage map ngay lập tức
                if (_mainVm != null)
                    await _mainVm.RefreshPoisFromApiAsync();
            }
        }
        finally { IsBusy = false; }
    }

    // Xóa POI
    [RelayCommand]
    public async Task DeletePoiAsync(PoiModel poi)
    {
        var confirmed = await ConfirmDelete?.Invoke(
            $"Xóa quán \"{poi.Name}\"?")!;
        if (!confirmed) return;

        IsBusy = true;
        try
        {
            var ok = await _adminSvc.DeletePoiAsync(poi.Id);
            StatusText = ok ? $"✅ Đã xóa {poi.Name}" : "❌ Xóa thất bại";
            if (ok)
            {
                await LoadPoisAsync();
                if (_mainVm != null)
                    await _mainVm.RefreshPoisFromApiAsync();
            }
        }
        finally { IsBusy = false; }
    }

    // Ẩn/hiện POI
    [RelayCommand]
    public async Task TogglePoiAsync(PoiModel poi)
    {
        var ok = await _adminSvc.TogglePoiVisibilityAsync(poi.Id);
        if (ok) await LoadPoisAsync();
    }

    [RelayCommand]
    public void CancelPoiForm() => IsPoiFormVisible = false;

    // ══════════════════════════════════════════
    // USER MANAGEMENT
    // ══════════════════════════════════════════

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        var list = await _adminSvc.GetAllUsersAsync();
        Users.Clear();
        foreach (var u in list) Users.Add(u);
    }

    // Khóa/mở tài khoản
    [RelayCommand]
    public async Task ToggleUserAsync(UserModel user)
    {
        // Không cho khóa chính mình
        if (user.Username == _apiSvc.Username)
        {
            ShowAlert?.Invoke("Không thể khóa tài khoản đang đăng nhập!");
            return;
        }
        var ok = await _adminSvc.ToggleUserActiveAsync(user.Id);
        if (ok) await LoadUsersAsync();
    }

    // Xóa user
    [RelayCommand]
    public async Task DeleteUserAsync(UserModel user)
    {
        if (user.Username == _apiSvc.Username)
        {
            ShowAlert?.Invoke("Không thể xóa tài khoản đang đăng nhập!");
            return;
        }

        var confirmed = await ConfirmDelete?.Invoke(
            $"Xóa tài khoản \"{user.Username}\"?")!;
        if (!confirmed) return;

        IsBusy = true;
        try
        {
            var ok = await _adminSvc.DeleteUserAsync(user.Id);
            StatusText = ok ? $"✅ Đã xóa {user.Username}" : "❌ Xóa thất bại";
            if (ok) await LoadUsersAsync();
        }
        finally { IsBusy = false; }
    }

    // ── Admin tạo user mới (không logout admin) ──────────────────
    public async Task<(bool Success, string Message)> CreateUserFromAdminAsync(
        string username, string email, string password, string role)
    {
        IsBusy = true;
        try
        {
            var (ok, msg) = await _adminSvc.CreateUserAsync(
                username, email, password, role);
            StatusText = ok ? $"✅ {msg}" : $"❌ {msg}";
            return (ok, msg);
        }
        finally { IsBusy = false; }
    }

    // ── Tab switch ────────────────────────────────────────────────
    [RelayCommand]
    public void SwitchToPois()
    {
        IsPoisTab  = true;
        IsUsersTab = false;
    }

    [RelayCommand]
    public void SwitchToUsers()
    {
        IsPoisTab  = false;
        IsUsersTab = true;
    }
}
