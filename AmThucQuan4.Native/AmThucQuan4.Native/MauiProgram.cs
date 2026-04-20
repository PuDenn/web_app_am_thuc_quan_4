using AmThucQuan4.Native.Services;
using AmThucQuan4.Native.ViewModels;
using AmThucQuan4.Native.Views;
using Plugin.Maui.Audio;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace AmThucQuan4.Native;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        // ── HTTP Clients ──────────────────────────────────────────
        // ApiService: gọi backend API (Auth + POIs)
        builder.Services.AddHttpClient<ApiService>();
        builder.Services.AddSingleton<IApiService>(sp =>
            new ApiService(sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(ApiService))));

        // MapService: OSRM routing + Nominatim geocoding
        builder.Services.AddHttpClient<MapService>();
        builder.Services.AddSingleton<IMapService>(sp =>
            new MapService(sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(MapService))));

        // ── PoiService: lấy POI từ API (có fallback offline) ────────
        builder.Services.AddHttpClient<PoiService>();
        builder.Services.AddSingleton<IPoiService>(sp =>
            new PoiService(
                sp.GetRequiredService<IHttpClientFactory>()
                  .CreateClient(nameof(PoiService)),
                sp.GetRequiredService<IApiService>()));

        // ── Services ──────────────────────────────────────────────
        builder.Services.AddSingleton<IQRService,       QRService>();
        builder.Services.AddSingleton<ILanguageService, LanguageService>();
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<IAudioService,   AudioService>();

        // ── Admin Service ─────────────────────────────────────────
        builder.Services.AddHttpClient<AdminService>();
        builder.Services.AddSingleton<IAdminService>(sp =>
            new AdminService(
                sp.GetRequiredService<IHttpClientFactory>()
                  .CreateClient(nameof(AdminService)),
                sp.GetRequiredService<IApiService>()));

        // ── ViewModels ────────────────────────────────────────────
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<AmThucQuan4.Native.ViewModels.Admin.AdminViewModel>();

        // ── Views ─────────────────────────────────────────────────
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AmThucQuan4.Native.Views.Admin.AdminPage>();

        return builder.Build();
    }
}
