using AmThucQuan4.Native.Views;

namespace AmThucQuan4.Native;

public partial class App : Application
{
    private readonly LoginPage _loginPage;

    public App(LoginPage loginPage)
    {
        InitializeComponent();
        _loginPage = loginPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // App mở LoginPage trước
        return new Window(_loginPage);
    }
}
