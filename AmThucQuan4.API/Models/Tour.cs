namespace AmThucQuan4.API.Models;

public class Tour
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = "";
    public string   Description { get; set; } = "";
    public string?  ImageUrl    { get; set; }
    public bool     IsActive    { get; set; } = true;
    public DateTime CreatedAt   { get; set; } = DateTime.Now;

    public List<TourPoi> TourPois { get; set; } = [];
}

public class TourPoi
{
    public int  Id             { get; set; }
    public int  TourId         { get; set; }
    public int  PoiId          { get; set; }
    public int  DisplayOrder   { get; set; }
    public int  GeofenceRadius { get; set; } = 30;

    public Tour? Tour { get; set; }
    public Poi?  Poi  { get; set; }
}
