using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmThucQuan4.API.Data;
using AmThucQuan4.API.Models;

namespace AmThucQuan4.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoisController : ControllerBase
{
    private readonly AppDbContext _db;
    public PoisController(AppDbContext db) => _db = db;

    // GET /api/pois — public, lấy tất cả POI đang hiển thị
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pois = await _db.POIs
            .Where(p => p.IsVisible)
            .OrderBy(p => p.Id)
            .ToListAsync();
        return Ok(pois);
    }

    // GET /api/pois/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var poi = await _db.POIs.FindAsync(id);
        return poi == null ? NotFound() : Ok(poi);
    }

    // POST /api/pois — chỉ Admin
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Poi poi)
    {
        poi.CreatedAt = poi.UpdatedAt = DateTime.Now;
        _db.POIs.Add(poi);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = poi.Id }, poi);
    }

    // PUT /api/pois/{id} — chỉ Admin
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Poi updated)
    {
        var poi = await _db.POIs.FindAsync(id);
        if (poi == null) return NotFound();

        poi.Name         = updated.Name;
        poi.Category     = updated.Category;
        poi.CategoryIcon = updated.CategoryIcon;
        poi.Description  = updated.Description;
        poi.Address      = updated.Address;
        poi.Hours        = updated.Hours;
        poi.PriceRange   = updated.PriceRange;
        poi.ImageUrl     = updated.ImageUrl;
        poi.AudioScript  = updated.AudioScript;
        poi.Latitude     = updated.Latitude;
        poi.Longitude    = updated.Longitude;
        poi.IsVisible    = updated.IsVisible;
        poi.UpdatedAt    = DateTime.Now;

        await _db.SaveChangesAsync();
        return Ok(poi);
    }

    // DELETE /api/pois/{id} — chỉ Admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var poi = await _db.POIs.FindAsync(id);
        if (poi == null) return NotFound();
        _db.POIs.Remove(poi);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PATCH /api/pois/{id}/toggle — Ẩn/hiện POI
    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Toggle(int id)
    {
        var poi = await _db.POIs.FindAsync(id);
        if (poi == null) return NotFound();
        poi.IsVisible = !poi.IsVisible;
        poi.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return Ok(new { id = poi.Id, isVisible = poi.IsVisible });
    }
}
