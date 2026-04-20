using Microsoft.EntityFrameworkCore;
using AmThucQuan4.API.Models;

namespace AmThucQuan4.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>    Users    { get; set; }
    public DbSet<Poi>     POIs     { get; set; }
    public DbSet<Tour>    Tours    { get; set; }
    public DbSet<TourPoi> TourPOIs { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Map đúng tên bảng trong SQL Server
        mb.Entity<User>   ().ToTable("Users");
        mb.Entity<Poi>    ().ToTable("POIs");
        mb.Entity<Tour>   ().ToTable("Tours");
        mb.Entity<TourPoi>().ToTable("TourPOIs");

        mb.Entity<TourPoi>()
            .HasOne(tp => tp.Tour)
            .WithMany(t => t.TourPois)
            .HasForeignKey(tp => tp.TourId);

        mb.Entity<TourPoi>()
            .HasOne(tp => tp.Poi)
            .WithMany()
            .HasForeignKey(tp => tp.PoiId);
    }
}
