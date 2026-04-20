namespace AmThucQuan4.Native.Services;

public interface IQRService
{
    /// <summary>Tạo ImageSource của QR code cho POI URL</summary>
    ImageSource GenerateQr(string poiId);

    /// <summary>URL đầy đủ mà QR trỏ tới</summary>
    string GetPoiWebUrl(string poiId);
}
