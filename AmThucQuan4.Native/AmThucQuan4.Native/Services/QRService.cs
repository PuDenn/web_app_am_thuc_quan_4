// Dùng API QR server miễn phí thay vì ZXing vì:
// 1. ZXing.Net.Maui có breaking change trên .NET 9, hay lỗi build
// 2. api.qrserver.com trả về PNG, không cần cài thêm package
// 3. App chỉ cần HIỂN THỊ QR (không scan), không cần thư viện nặng

namespace AmThucQuan4.Native.Services;

public class QRService : IQRService
{
    public string GetPoiWebUrl(string poiId)
        => $"{AppConstants.WEB_BASE_URL}/poi.html?id={poiId}";

    public ImageSource GenerateQr(string poiId)
    {
        var url     = GetPoiWebUrl(poiId);
        var encoded = Uri.EscapeDataString(url);
        var qrUrl   = $"https://api.qrserver.com/v1/create-qr-code/" +
                      $"?size=250x250&margin=10&data={encoded}";

        return ImageSource.FromUri(new Uri(qrUrl));
    }
}
