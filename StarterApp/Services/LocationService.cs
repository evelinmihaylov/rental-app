using Microsoft.Maui.Devices.Sensors;

namespace StarterApp.Services;

public class LocationService : ILocationService
{
    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10));

            return await Geolocation.Default.GetLocationAsync(request);
        }
        catch
        {
            return null;
        }
    }
}