using Microsoft.Maui.Devices.Sensors;

namespace StarterApp.Services;

public class LocationService : ILocationService
{
    public async Task<AppLocation?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10));

            var location = await Geolocation.Default.GetLocationAsync(request);

            return location is null
                ? null
                : new AppLocation(location.Latitude, location.Longitude);
        }
        catch
        {
            return null;
        }
    }
}
