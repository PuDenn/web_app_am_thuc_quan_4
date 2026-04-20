using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmThucQuan4.API.Data;
using AmThucQuan4.API.Models;

namespace AmThucQuan4.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    // GET /api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Select(u => new {
                u.Id, u.Username, u.Email,
                u.Role, u.CreatedAt, u.LastLoginAt, u.IsActive
            })
            .OrderBy(u => u.Id)
            .ToListAsync();
        return Ok(users);
    }

    // POST /api/users/create — Admin tạo user mới (không login vào account mới)
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email đã được sử dụng" });

        if (req.Password.Length < 6)
            return BadRequest(new { message = "Mật khẩu phải ít nhất 6 ký tự" });

        var user = new User
        {
            Username     = req.Username,
            Email        = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role         = req.Role == "Admin" ? "Admin" : "User",
            IsActive     = true,
            CreatedAt    = DateTime.Now,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Trả về thông tin user mới — KHÔNG trả về token
        return Ok(new {
            message  = $"Đã tạo tài khoản {user.Username} thành công",
            user.Id,
            user.Username,
            user.Email,
            user.Role,
        });
    }

    // PATCH /api/users/{id}/toggle — Khóa/mở tài khoản
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { id = user.Id, isActive = user.IsActive });
    }

    // DELETE /api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string Role);
