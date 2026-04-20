using AmThucQuan4.Native.Models;

namespace AmThucQuan4.Native.Services;

public interface IApiService
{
    // Auth
    Task<LoginResult?> LoginAsync(string username, string password);
    Task<LoginResult?> RegisterAsync(string username, string email, string password);

    // POIs
    Task<List<PoiModel>> GetPoisAsync();

    // State
    string? Token    { get; }
    string? Username { get; }
    string? Role     { get; }
    bool    IsLoggedIn { get; }
    bool    IsGuest    { get; }
    void    Logout();
    void    SetGuestMode();
}

public record LoginResult(
    string Token,
    string Username,
    string Email,
    string Role,
    int    UserId);
