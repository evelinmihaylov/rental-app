namespace StarterApp.Services;

/// <summary>
/// App-level location DTO used to keep ViewModels independent from MAUI device types.
/// </summary>
public sealed record AppLocation(double Latitude, double Longitude);
