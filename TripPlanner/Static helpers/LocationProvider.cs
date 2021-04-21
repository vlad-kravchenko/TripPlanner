using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace TripPlanner
{
    static class LocationProvider
    {
        public static void Init(string key)
        {
            GMapProvider.Language = LanguageType.Russian;
            GMapProviders.GoogleHybridMap.ApiKey = key;
        }

        public static PointLatLng? GetCoordinates(string keyword)
        {
            var response = GMapProviders.GoogleHybridMap.GetPoint(keyword, out GeoCoderStatusCode statusCode);
            return response ?? null;
        }

        public static Placemark? GetPlacemark(PointLatLng location)
        {
            List<Placemark> plc = null;
            var st = GMapProviders.GoogleHybridMap.GetPlacemarks(location, out plc);
            plc.Reverse();
            return plc.FirstOrDefault(p => !string.IsNullOrEmpty(p.LocalityName));
        }

        public static Placemark? GetPlacemark(string keyword)
        {
            PointLatLng? loc = GMapProviders.GoogleHybridMap.GetPoint(keyword, out GeoCoderStatusCode statusCode).Value;
            if (!loc.HasValue) return null;

            List<Placemark> plc = null;
            var st = GMapProviders.GoogleHybridMap.GetPlacemarks(loc.Value, out plc);
            plc.Reverse();
            return plc.FirstOrDefault(p => !string.IsNullOrEmpty(p.LocalityName));
        }

        public static GMapRoute GetRoute(PointLatLng start, PointLatLng end)
        {
            GDirections dirs = null;
            var r = GMapProviders.GoogleHybridMap.GetDirections(out dirs, start, end, false, false, false, false, true);
            if (r == DirectionsStatusCode.OK)
            {
                MapRoute route = new MapRoute(dirs.Route, "My Trip");
                GMapRoute gmRoute = new GMapRoute(route.Points);
                gmRoute.Shape = new System.Windows.Shapes.Path() { Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = 4 };
                return gmRoute;
            }
            return null;
        }

        public static GMapRoute GetRoute(List<PointLatLng> points)
        {
            GMapRoute gmRoute = new GMapRoute(points);
            gmRoute.Shape = new System.Windows.Shapes.Path() { Stroke = new SolidColorBrush(Colors.Red), StrokeThickness = 4 };
            return gmRoute;
        }

        public static List<PointLatLng> GetRoutePoints(PointLatLng start, PointLatLng end)
        {
            GDirections dirs = null;
            var r = GMapProviders.GoogleHybridMap.GetDirections(out dirs, start, end, false, false, false, false, true);
            if (r == DirectionsStatusCode.OK)
                return dirs.Route;
            return new List<PointLatLng>();
        }

        public static uint GetDistance(PointLatLng? start, PointLatLng? end)
        {
            GDirections dirs = null;
            var r = GMapProviders.GoogleHybridMap.GetDirections(out dirs, start.Value, end.Value, false, false, false, false, true);
            if (r == DirectionsStatusCode.OK)
                return dirs.DistanceValue;
            return 0;
        }
    }
}