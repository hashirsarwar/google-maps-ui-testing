namespace GoogleMaps.Tests.Data;

public struct Coordinates {
    public readonly float Latitude;
    public readonly float Longitude;
    public readonly float ZoomLevel;

    public Coordinates(float latitude, float longitude, float zoomLevel) {
        Latitude = latitude;
        Longitude = longitude;
        ZoomLevel = zoomLevel;
    }
}