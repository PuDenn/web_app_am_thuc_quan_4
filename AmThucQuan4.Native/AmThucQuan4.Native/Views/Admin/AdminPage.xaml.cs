using AmThucQuan4.Native.Converters;
using AmThucQuan4.Native.Services;
using AmThucQuan4.Native.ViewModels.Admin;

namespace AmThucQuan4.Native.Views.Admin;

public partial class AdminPage : ContentPage
{
    private readonly AdminViewModel _vm;

    public AdminPage(AdminViewModel vm,
        AmThucQuan4.Native.ViewModels.MainViewModel mainVm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;

        // Inject MainViewModel để trigger map refresh sau khi lưu POI
        _vm.SetMainViewModel(mainVm);

        // Lắng nghe events từ ViewModel
        _vm.ShowAlert     += msg => MainThread.BeginInvokeOnMainThread(async () =>
            await DisplayAlert("Thông báo", msg, "OK"));

        _vm.ConfirmDelete += async msg =>
        {
            bool result = false;
            await MainThread.InvokeOnMainThreadAsync(async () =>
                result = await DisplayAlert("Xác nhận xóa", msg, "Xóa", "Hủy"));
            return result;
        };

        Loaded += async (_, _) => await _vm.LoadAsync();
    }

    // ─── Tab switch ──────────────────────────────────────────────
    private void OnPoisTabClicked(object? sender, EventArgs e)
    {
        _vm.SwitchToPoisCommand.Execute(null);
        btnPoisTab.BackgroundColor  = Color.FromArgb("#f97316");
        btnPoisTab.TextColor        = Colors.White;
        btnUsersTab.BackgroundColor = Color.FromArgb("#1e293b");
        btnUsersTab.TextColor       = Color.FromArgb("#94a3b8");
    }

    private void OnUsersTabClicked(object? sender, EventArgs e)
    {
        _vm.SwitchToUsersCommand.Execute(null);
        btnUsersTab.BackgroundColor = Color.FromArgb("#7c3aed");
        btnUsersTab.TextColor       = Colors.White;
        btnPoisTab.BackgroundColor  = Color.FromArgb("#1e293b");
        btnPoisTab.TextColor        = Color.FromArgb("#94a3b8");
    }

    // ─── Về lại MainPage ─────────────────────────────────────────
    private void OnBackToApp(object? sender, EventArgs e)
    {
        var mainPage = Handler?.MauiContext?.Services
            .GetRequiredService<Views.MainPage>();
        if (mainPage != null && Window != null)
            Window.Page = mainPage;
    }

    // ─── Chọn ảnh từ điện thoại ─────────────────────────────────
    public async void OnPickImageClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(
                new MediaPickerOptions
                {
                    Title = "Chọn ảnh quán ăn"
                });

            if (result == null) return;

            // Lưu file vào thư mục cache app
            var cacheDir   = FileSystem.CacheDirectory;
            var fileName   = $"poi_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(result.FileName)}";
            var localPath  = Path.Combine(cacheDir, fileName);

            using var srcStream  = await result.OpenReadAsync();
            using var destStream = File.Create(localPath);
            await srcStream.CopyToAsync(destStream);

            // Cập nhật ImageUrl = local file path
            var fileUrl = $"file://{localPath}";
            _vm.NotifyImageUrlChanged(fileUrl);

            // Cập nhật preview ngay
            if (imgPoiPreview != null)
                imgPoiPreview.Source = ImageSource.FromFile(localPath);

            await DisplayAlert("✅", "Đã chọn ảnh thành công!", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("❌ Quyền bị từ chối",
                "Vui lòng cấp quyền truy cập thư viện ảnh trong Cài đặt.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Lỗi", ex.Message, "OK");
        }
    }

    // ─── URL ảnh thay đổi → cập nhật preview ─────────────────────
    public void OnImageUrlChanged(object? sender, TextChangedEventArgs e)
    {
        var url = e.NewTextValue;
        _vm.NotifyImageUrlChanged(url);

        if (imgPoiPreview != null && !string.IsNullOrWhiteSpace(url))
        {
            try { imgPoiPreview.Source = ImageSource.FromUri(new Uri(url)); }
            catch { /* invalid URI — ignore */ }
        }
    }

    // ─── Chọn vị trí trên bản đồ ────────────────────────────────
    public async void OnPickLocationClicked(object? sender, EventArgs e)
    {
        // Hướng dẫn user nhập tọa độ từ Google Maps
        await DisplayAlert(
            "🗺️ Chọn vị trí",
            "Cách lấy tọa độ từ Google Maps:\n\n" +
            "1. Mở Google Maps trên máy tính\n" +
            "2. Tìm địa điểm muốn thêm\n" +
            "3. Chuột phải vào vị trí → copy tọa độ\n" +
            "4. Dán vào ô Latitude và Longitude\n\n" +
            "Ví dụ Quận 4: 10.7575, 106.7012",
            "Đã hiểu");
    }

    // ─── Thêm User mới ──────────────────────────────────────────
    public async void OnAddUserClicked(object? sender, EventArgs e)
    {
        // Username
        var username = await DisplayPromptAsync(
            "Thêm User", "Tên đăng nhập:",
            placeholder: "username");
        if (string.IsNullOrWhiteSpace(username)) return;

        // Email
        var email = await DisplayPromptAsync(
            "Thêm User", "Email:",
            placeholder: "email@example.com",
            keyboard: Keyboard.Email);
        if (string.IsNullOrWhiteSpace(email)) return;

        // Password
        var password = await DisplayPromptAsync(
            "Thêm User", "Mật khẩu (min 6 ký tự):",
            placeholder: "••••••");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            await DisplayAlert("Lỗi", "Mật khẩu phải ít nhất 6 ký tự", "OK");
            return;
        }

        // Role
        var role = await DisplayActionSheet(
            "Chọn vai trò", "Hủy", null, "User", "Admin");
        if (role == "Hủy" || string.IsNullOrEmpty(role)) return;

        // Gọi AdminService.CreateUserAsync — KHÔNG ghi đè token Admin
        var (success, message) = await _vm.CreateUserFromAdminAsync(
            username, email, password, role);

        if (success)
        {
            await DisplayAlert("✅ Thành công", message, "OK");
            await _vm.LoadUsersAsync();
        }
        else
        {
            await DisplayAlert("❌ Thất bại", message, "OK");
        }
    }
}
