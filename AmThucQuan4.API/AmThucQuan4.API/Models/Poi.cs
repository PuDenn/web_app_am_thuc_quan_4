namespace AmThucQuan4.API.Models;

public class Poi
{
    public int      Id           { get; set; }
    public string   Name         { get; set; } = "";
    public string   Category     { get; set; } = "";
    public string   CategoryIcon { get; set; } = "🍽️";
    public string   Description  { get; set; } = "";
    public string   Address      { get; set; } = "";
    public string   Hours        { get; set; } = "";
    public string   PriceRange   { get; set; } = "";
    public string?  ImageUrl     { get; set; }
    public string?  AudioPath    { get; set; }
    public string?  AudioScript  { get; set; }
    public double   Latitude     { get; set; }
    public double   Longitude    { get; set; }
    public bool     IsVisible    { get; set; } = true;
    public DateTime CreatedAt    { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt   { get; set; }  // nullable — tránh crash khi cột NULL
}
