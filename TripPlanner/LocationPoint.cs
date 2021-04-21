using GMap.NET;

namespace TripPlanner
{
    class LocationPoint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public PointLatLng? Coordinates { get; set; }
        public uint Distance { get; set; }
    }
}
