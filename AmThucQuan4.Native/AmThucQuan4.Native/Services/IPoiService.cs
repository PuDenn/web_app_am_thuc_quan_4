using AmThucQuan4.Native.Models;

namespace AmThucQuan4.Native.Services;

public interface IPoiService
{
    Task<List<PoiModel>> GetAllAsync();
    Task<PoiModel?> GetByIdAsync(string id);
}
