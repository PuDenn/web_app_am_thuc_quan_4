using Android.App;
using Android.Runtime;

namespace AmThucQuan4.Native;

[Application(Name = "com.amthucquan4.maui.MainApplication")]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership) { }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
