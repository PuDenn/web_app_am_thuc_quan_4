namespace AmThucQuan4.API.DTOs;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password);
public record AuthResponse(
    string Token,
    string Username,
    string Email,
    string Role,
    int    UserId);
