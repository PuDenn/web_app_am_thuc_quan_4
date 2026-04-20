namespace AmThucQuan4.Native.Services;

public interface IMapService
{
    Task<List<(double Lat, double Lng)>> GetRouteAsync(
        double fromLat, double fromLng,
        double toLat,   double toLng);

    Task<(double Lat, double Lng)?> GeocodeAsync(string address);
}
