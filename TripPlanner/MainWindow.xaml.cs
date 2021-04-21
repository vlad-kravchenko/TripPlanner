using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TripPlanner
{

    public partial class MainWindow : Window
    {
        readonly string key = "AIzaSyDPgO6CLNKi2Rf0zHstqDE-2YeS9967cgA";
        List<LocationPoint> points = new List<LocationPoint>();

        public MainWindow()
        {
            InitializeComponent();

            if (!NetworkTester.CheckForInternetConnection()) MessageBox.Show("Sorry, unable to establish connection!");

            Map.MapProvider = GMapProviders.GoogleHybridMap;
            Map.Position = new PointLatLng(46,37);
            Map.CanDragMap = true;
            Map.DragButton = MouseButton.Left;
            Map.ShowCenter = false;
            LocationProvider.Init(key);
        }

        private void AddMarker(System.Drawing.Bitmap markerPic, PointLatLng location)
        {
            GMapMarker marker = new GMapMarker(location);
            var image = new Image();
            image.Width = 20;
            image.Height = 50;
            image.Source = BitmapHelper.BitmapToSource(markerPic);
            marker.Shape = image;
            marker.Offset = new Point(-5, -40);
            Map.Markers.Add(marker);
        }

        private void Map_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            double lat = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y).Lat;
            double lng = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y).Lng;
            var location = new PointLatLng(lat, lng);
            NextLocation.Text = LocationProvider.GetPlacemark(location)?.LocalityName;
            if (!string.IsNullOrEmpty(NextLocation.Text))
                AddMarker(Properties.Resources.red, location);
        }

        private void AddLocation_Click(object sender, RoutedEventArgs e)
        {
            var coordinates = LocationProvider.GetCoordinates(NextLocation.Text);
            if (coordinates == null) return;
            var placemark = LocationProvider.GetPlacemark(NextLocation.Text);
            if (placemark == null) return;
            var point = new LocationPoint
            {
                Id = points.Count > 0 ? points.Max(p => p.Id) + 1 : 1,
                Coordinates = coordinates,
                Country = placemark.Value.CountryName,
                Name = placemark.Value.LocalityName
            };
            if (points.Count > 0)
            {
                point.Distance = LocationProvider.GetDistance(points.Last().Coordinates, point.Coordinates) / 1000;
            }
            points.Add(point);
            TripTable.ItemsSource = null;
            TripTable.ItemsSource = points;
            NextLocation.Text = "";
            DisplayRoute(points);
        }

        private void ShowAllRoute_Click(object sender, RoutedEventArgs e)
        {
            DisplayRoute(points);
        }

        private void DisplayRoute(List<LocationPoint> locations)
        {
            if (locations.Count == 0) return;
            List<PointLatLng> routePoints = new List<PointLatLng>();
            for (int i = 0; i < locations.Count - 1; i++)
            {
                routePoints.AddRange(LocationProvider.GetRoutePoints(locations[i].Coordinates.Value, locations[i + 1].Coordinates.Value));
            }
            GMapRoute route = LocationProvider.GetRoute(routePoints);
            Map.Markers.Clear();
            Map.Markers.Add(route);
            SelectedPath.Text = $"Selected path: {locations.Sum(l => l.Distance) - locations.First().Distance}";
            if (points.Count == 0) return;
            foreach (var point in points)
            {
                AddMarker(Properties.Resources.blue, point.Coordinates.Value);
            }
            OveralPath.Text = $"Overal path: {points.Sum(p => p.Distance)}";
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var location = (sender as DataGridRow).Item as LocationPoint;
            Map.Position = location.Coordinates.Value;
        }

        private void TripTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<LocationPoint> selectedRows = new List<LocationPoint>();
            foreach (var item in TripTable.SelectedItems) selectedRows.Add(item as LocationPoint);
            if (selectedRows.Count < 2) return;

            selectedRows = selectedRows.OrderBy(r => r.Id).ToList();
            bool result = true;
            for (int i = 0; i < selectedRows.Count - 1; i++)
            {
                if (selectedRows[i].Id + 1 != selectedRows[i + 1].Id) result = false;
            }
            if (result)
            {
                DisplayRoute(selectedRows);
            }
            else
            {
                DisplayRoute(points);
            }
        }

        private void NextLocation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddLocation_Click(null, null);
        }

        private void TripTable_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (TripTable.SelectedItems.Count != 1) return;
            if (e.Key != Key.Delete) return;
            var item = TripTable.SelectedItem as LocationPoint;
            if (MessageBox.Show("Are you sure you want to remove location from your trip?", "Removing of the point", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                points.Remove(item);
                TripTable.ItemsSource = null;
                TripTable.ItemsSource = points;
                DisplayRoute(points);
            }
        }
    }
}