using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmThucQuan4.API.Data;
using AmThucQuan4.API.Models;

namespace AmThucQuan4.API.Controllers;

/// <summary>
/// Controller setup 1 lần — dùng để tạo admin account đúng hash
/// XÓA controller này sau khi setup xong!
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly AppDbContext _db;
    public SetupController(AppDbContext db) => _db = db;

    // GET /api/setup/init
    // Gọi 1 lần để tạo/reset admin account
    [HttpGet("init")]
    public async Task<IActionResult> Init()
    {
        // Xóa admin cũ nếu có
        var existing = await _db.Users
            .Where(u => u.Username == "admin")
            .ToListAsync();
        _db.Users.RemoveRange(existing);

        // Tạo admin mới với BCrypt hash đúng
        var admin = new User
        {
            Username     = "admin",
            Email        = "admin@amthucquan4.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role         = "Admin",
            IsActive     = true,
            CreatedAt    = DateTime.Now,
        };

        _db.Users.Add(admin);
        await _db.SaveChangesAsync();

        return Ok(new {
            message  = "Admin account created successfully!",
            username = "admin",
            password = "Admin@123",
            hash     = admin.PasswordHash
        });
    }
}
