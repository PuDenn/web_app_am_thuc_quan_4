using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmThucQuan4.API.Data;
using AmThucQuan4.API.Models;

namespace AmThucQuan4.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly AppDbContext _db;
    public ToursController(AppDbContext db) => _db = db;

    // GET /api/tours
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tours = await _db.Tours
            .Where(t => t.IsActive)
            .Include(t => t.TourPois)
                .ThenInclude(tp => tp.Poi)
            .OrderBy(t => t.Id)
            .ToListAsync();
        return Ok(tours);
    }

    // GET /api/tours/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tour = await _db.Tours
            .Include(t => t.TourPois.OrderBy(tp => tp.DisplayOrder))
                .ThenInclude(tp => tp.Poi)
            .FirstOrDefaultAsync(t => t.Id == id);
        return tour == null ? NotFound() : Ok(tour);
    }
}
