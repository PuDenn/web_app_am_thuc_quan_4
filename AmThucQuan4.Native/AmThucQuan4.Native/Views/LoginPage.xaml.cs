using AmThucQuan4.Native.Services;
using AmThucQuan4.Native.ViewModels;

namespace AmThucQuan4.Native.Views;

public partial class LoginPage : ContentPage
{
    private readonly IApiService   _api;
    private readonly MainViewModel _mainVm;

    public LoginPage(IApiService api, MainViewModel mainVm)
    {
        InitializeComponent();
        _api    = api;
        _mainVm = mainVm;
    }

    // ─── Tab switch ──────────────────────────────────────────────
    private void OnLoginTabClicked(object? sender, EventArgs e)
    {
        loginForm.IsVisible    = true;
        registerForm.IsVisible = false;
        btnLoginTab.BackgroundColor    = Color.FromArgb("#f97316");
        btnLoginTab.TextColor          = Colors.White;
        btnRegisterTab.BackgroundColor = Color.FromArgb("#1e293b");
        btnRegisterTab.TextColor       = Color.FromArgb("#94a3b8");
    }

    private void OnRegisterTabClicked(object? sender, EventArgs e)
    {
        loginForm.IsVisible    = false;
        registerForm.IsVisible = true;
        btnRegisterTab.BackgroundColor = Color.FromArgb("#7c3aed");
        btnRegisterTab.TextColor       = Colors.White;
        btnLoginTab.BackgroundColor    = Color.FromArgb("#1e293b");
        btnLoginTab.TextColor          = Color.FromArgb("#94a3b8");
    }

    // ─── Nút Đăng nhập bấm ──────────────────────────────────────
    private async void OnLoginClicked(object? sender, EventArgs e)
        => await DoLoginAsync();

    protected override bool OnBackButtonPressed() => true;

    private async Task DoLoginAsync()
    {
        var username = txtUsername.Text?.Trim() ?? "";
        var password = txtPassword.Text ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError(lblError, "Vui lòng nhập đầy đủ thông tin");
            return;
        }

        SetLoading(true);
        lblError.IsVisible = false;

        try
        {
            var result = await _api.LoginAsync(username, password);

            if (result == null)
            {
                ShowError(lblError, "Sai tên đăng nhập hoặc mật khẩu.\nKiểm tra API đang chạy chưa?");
                return;
            }

            await NavigateToMainAsync();
        }
        catch (Exception ex)
        {
            ShowError(lblError, $"Lỗi kết nối API:\n{ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[Login] {ex}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ─── Register ────────────────────────────────────────────────
    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        var username = txtRegUsername.Text?.Trim() ?? "";
        var email    = txtRegEmail.Text?.Trim() ?? "";
        var password = txtRegPassword.Text ?? "";

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(email)    ||
            string.IsNullOrEmpty(password))
        {
            ShowError(lblRegError, "Vui lòng nhập đầy đủ thông tin");
            return;
        }

        if (password.Length < 6)
        {
            ShowError(lblRegError, "Mật khẩu phải ít nhất 6 ký tự");
            return;
        }

        SetLoading(true);
        try
        {
            var result = await _api.RegisterAsync(username, email, password);
            if (result == null)
            {
                ShowError(lblRegError, "Đăng ký thất bại, thử username khác");
                return;
            }
            await NavigateToMainAsync();
        }
        catch (Exception ex)
        {
            ShowError(lblRegError, $"Lỗi: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ─── Navigate đến MainPage ───────────────────────────────────
    private async Task NavigateToMainAsync()
    {
        try
        {
            // Load POI từ API
            var pois = await _api.GetPoisAsync();
            if (pois.Count > 0)
            {
                _mainVm.Pois.Clear();
                foreach (var p in pois) _mainVm.Pois.Add(p);
            }

            // Chuyển sang MainPage
            var mainPage = Handler?.MauiContext?.Services
                .GetRequiredService<MainPage>();

            if (mainPage != null && Window != null)
                Window.Page = mainPage;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavToMain] {ex.Message}");
            await DisplayAlert("Lỗi", ex.Message, "OK");
        }
    }

    // ─── Khách vào không cần đăng nhập ─────────────────────────
    private async void OnGuestClicked(object? sender, EventArgs e)
    {
        SetLoading(true);
        try
        {
            // Đánh dấu guest mode (không có token)
            _api.SetGuestMode();
            await NavigateToMainAsync();
        }
        catch (Exception ex)
        {
            ShowError(lblError, $"Lỗi: {ex.Message}");
        }
        finally { SetLoading(false); }
    }

    private void ShowError(Label lbl, string msg)
    {
        lbl.Text      = msg;
        lbl.IsVisible = true;
    }

    private void SetLoading(bool loading)
    {
        loadingIndicator.IsRunning = loading;
        loadingIndicator.IsVisible = loading;
    }
}
