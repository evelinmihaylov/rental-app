namespace StarterApp.Services;

public interface ILocationService
{
    Task<AppLocation?> GetCurrentLocationAsync();
}
