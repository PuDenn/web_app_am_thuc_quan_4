using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmThucQuan4.API.Data;

namespace AmThucQuan4.API.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminStatsController(AppDbContext db) => _db = db;

    // GET /api/admin/stats/overview
    [HttpGet("overview")]
    public async Task<IActionResult> Overview()
    {
        var today = DateTime.Today;
        var totalPOIs       = await _db.POIs.CountAsync();
        var totalVisitsToday = await _db.AccessLogs
            .CountAsync(l => l.Type == "view" && l.Timestamp >= today);
        var totalAudioPlays  = await _db.AccessLogs
            .CountAsync(l => l.Type == "audio_play" && l.Timestamp >= today);
        var totalQRScans     = await _db.AccessLogs
            .CountAsync(l => l.Type == "qr_scan" && l.Timestamp >= today);

        return Ok(new { totalPOIs, totalVisitsToday, totalAudioPlays, totalQRScans });
    }

    // GET /api/admin/stats/poi/{id}
    [HttpGet("poi/{id}")]
    public async Task<IActionResult> PoiStats(int id)
    {
        var poi = await _db.POIs.FindAsync(id);
        if (poi == null) return NotFound();

        var logs = await _db.AccessLogs
            .Where(l => l.PoiId == id)
            .ToListAsync();

        var viewsByDate = logs
            .Where(l => l.Type == "view")
            .GroupBy(l => l.Timestamp.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .ToList();

        return Ok(new
        {
            poiName       = poi.Name,
            viewCount      = logs.Count(l => l.Type == "view"),
            audioPlayCount = logs.Count(l => l.Type == "audio_play"),
            qrScanCount    = logs.Count(l => l.Type == "qr_scan"),
            viewsByDate,
        });
    }

    // GET /api/admin/stats/top-poi?limit=5
    [HttpGet("top-poi")]
    public async Task<IActionResult> TopPoi([FromQuery] int limit = 5)
    {
        var result = await _db.AccessLogs
            .Where(l => l.Type == "view")
            .GroupBy(l => l.PoiId)
            .Select(g => new { poiId = g.Key, viewCount = g.Count() })
            .OrderByDescending(x => x.viewCount)
            .Take(limit)
            .ToListAsync();

        // Join với POI name
        var poiIds = result.Select(x => x.poiId).ToList();
        var pois   = await _db.POIs
            .Where(p => poiIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var logs = await _db.AccessLogs
            .Where(l => poiIds.Contains(l.PoiId))
            .ToListAsync();

        var final = result.Select(x => new
        {
            poiId      = x.poiId,
            poiName    = pois.GetValueOrDefault(x.poiId, "Unknown"),
            viewCount  = x.viewCount,
            audioCount = logs.Count(l => l.PoiId == x.poiId && l.Type == "audio_play"),
            qrCount    = logs.Count(l => l.PoiId == x.poiId && l.Type == "qr_scan"),
        });
        return Ok(final);
    }

    // GET /api/admin/stats/timeline?from=2024-01-01&to=2024-01-31
    [HttpGet("timeline")]
    public async Task<IActionResult> Timeline(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var start = from ?? DateTime.Today.AddDays(-7);
        var end   = to   ?? DateTime.Today.AddDays(1);

        var logs = await _db.AccessLogs
            .Where(l => l.Timestamp >= start && l.Timestamp <= end)
            .ToListAsync();

        var timeline = logs
            .GroupBy(l => l.Timestamp.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                date       = g.Key.ToString("yyyy-MM-dd"),
                views      = g.Count(l => l.Type == "view"),
                audioPlays = g.Count(l => l.Type == "audio_play"),
                qrScans    = g.Count(l => l.Type == "qr_scan"),
            });
        return Ok(timeline);
    }
}
