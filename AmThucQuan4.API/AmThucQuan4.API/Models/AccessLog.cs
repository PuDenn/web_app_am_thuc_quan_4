namespace AmThucQuan4.API.Models;

public class AccessLog
{
    public int      Id        { get; set; }
    public int      PoiId     { get; set; }
    public string   Type      { get; set; } = ""; // view | audio_play | qr_scan
    public string   Source    { get; set; } = ""; // app | web
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
