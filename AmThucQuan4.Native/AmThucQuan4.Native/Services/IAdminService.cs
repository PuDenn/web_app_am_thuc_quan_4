using AmThucQuan4.Native.Models;

namespace AmThucQuan4.Native.Services;

public interface IAdminService
{
    // POI Management
    Task<List<PoiModel>> GetAllPoisAsync();
    Task<bool> CreatePoiAsync(PoiModel poi);
    Task<bool> UpdatePoiAsync(PoiModel poi);
    Task<bool> DeletePoiAsync(string id);
    Task<bool> TogglePoiVisibilityAsync(string id);

    // User Management
    Task<List<UserModel>> GetAllUsersAsync();
    Task<(bool Success, string Message)> CreateUserAsync(
        string username, string email, string password, string role);
    Task<bool> ToggleUserActiveAsync(int id);
    Task<bool> DeleteUserAsync(int id);
}

public class UserModel
{
    public int      Id          { get; set; }
    public string   Username    { get; set; } = "";
    public string   Email       { get; set; } = "";
    public string   Role        { get; set; } = "";
    public bool     IsActive    { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public string StatusText => IsActive ? "✅ Hoạt động" : "🔒 Bị khóa";
    public string StatusColor => IsActive ? "#22c55e" : "#ef4444";
    public string LastLoginText => LastLoginAt.HasValue
        ? LastLoginAt.Value.ToString("dd/MM/yyyy HH:mm")
        : "Chưa đăng nhập";
}
