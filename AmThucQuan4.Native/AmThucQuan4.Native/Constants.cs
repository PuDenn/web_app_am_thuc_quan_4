namespace AmThucQuan4.Native;

/// <summary>
/// Cấu hình URL — chỉnh WEB_BASE_URL thành IP LAN máy PC.
/// API vẫn dùng 10.0.2.2 cho emulator.
/// WEB (Web PWA) dùng IP LAN thật để điện thoại quét QR vào được.
/// </summary>
public static class AppConstants
{
    // ── API: emulator dùng 10.0.2.2 = localhost máy tính ────────
    public const string API_BASE_URL = "http://10.0.2.2:5000";

    // ── Web PWA: LUÔN dùng IP LAN thật ──────────────────────────
    // Vì QR sẽ được điện thoại thật quét → phải là IP LAN
    // ⚠️ Đổi IP này thành IP LAN thật của máy PC
    public const string WEB_BASE_URL = "http://192.168.1.5:3000";
}
