using Microsoft.AspNetCore.Mvc;
using AmThucQuan4.API.Data;
using AmThucQuan4.API.Models;

namespace AmThucQuan4.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly AppDbContext _db;
    public LogController(AppDbContext db) => _db = db;

    // POST /api/log — Web PWA và App gọi khi có sự kiện
    [HttpPost]
    public async Task<IActionResult> Log([FromBody] LogRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Type) ||
            string.IsNullOrWhiteSpace(req.Source))
            return BadRequest();

        _db.AccessLogs.Add(new AccessLog
        {
            PoiId     = req.PoiId,
            Type      = req.Type,   // view | audio_play | qr_scan
            Source    = req.Source, // app | web
            Timestamp = DateTime.Now,
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "logged" });
    }
}

public record LogRequest(int PoiId, string Type, string Source);
