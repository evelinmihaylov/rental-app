using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Maui.ApplicationModel;
using StarterApp.Database.Models;

namespace StarterApp.Services;

public class ApiAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly INavigationService _navigationService;
    private User? _currentUser;
    private readonly List<string> _currentUserRoles = new();
    private bool _isHandlingSessionExpiry;

    public event EventHandler<bool>? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser != null;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    public ApiAuthenticationService(HttpClient httpClient, INavigationService navigationService)
    {
        _httpClient = httpClient;
        _navigationService = navigationService;
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        Console.WriteLine(" USING API AUTH");
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/token", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new AuthenticationResult(false, error?.Message ?? "Login failed");
            }

            var authToken = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (authToken == null)
            {
            return new AuthenticationResult(false, "Login failed: empty token response");
            }
            await SecureStorage.SetAsync("auth_token", authToken.Token);   
            await SecureStorage.SetAsync("user_id", authToken.UserId.ToString()); 
            await SecureStorage.SetAsync("auth_expires_at", authToken.ExpiresAt.ToString("O"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken.Token);

            var meResponse = await _httpClient.GetAsync("users/me");

            if (!meResponse.IsSuccessStatusCode)   
            {
            return new AuthenticationResult(false, "Failed to load user profile");
            }
            var profile = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();

            if (profile == null)   
            {
            return new AuthenticationResult(false, "Failed to parse user profile");
            }

            _currentUser = new User
            {
                Id = profile!.Id,
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                CreatedAt = profile.CreatedAt,
                IsActive = true
            };

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            var body = await response.Content.ReadAsStringAsync();
            

            if (!response.IsSuccessStatusCode)
            {
                return new AuthenticationResult(false, $"Registration failed ({(int)response.StatusCode}): {body}");
            }

            return new AuthenticationResult(true, "Registration successful. Please log in.");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = null;

        SecureStorage.Remove("auth_token");        
        SecureStorage.Remove("user_id");          
        SecureStorage.Remove("auth_expires_at");
        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public async Task<bool> EnsureValidSessionAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        var expiresAtRaw = await SecureStorage.GetAsync("auth_expires_at");

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expiresAtRaw))
        {
            return false;
        }

        if (!DateTime.TryParse(
                expiresAtRaw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var expiresAt))
        {
            return false;
        }

        if (DateTime.UtcNow >= expiresAt)
        {
            return false;
        }

        if (_httpClient.DefaultRequestHeaders.Authorization?.Parameter != token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return true;
    }

    public async Task HandleSessionExpiredAsync()
    {
        if (_isHandlingSessionExpiry)
        {
            return;
        }

        _isHandlingSessionExpiry = true;

        try
        {
            await LogoutAsync();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _navigationService.NavigateToRootAsync();

                Page? page = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page is NavigationPage navigationPage)
                {
                    page = navigationPage.CurrentPage;
                }

                if (page is not null)
                {
                    await page.DisplayAlert(
                        "Session expired",
                        "Your session has expired. Please log in again.",
                        "OK");
                }
            });
        }
        finally
        {
            _isHandlingSessionExpiry = false;
        }
    }

    public bool HasRole(string roleName) =>
        _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public bool HasAnyRole(params string[] roleNames) =>
        roleNames.Any(HasRole);

    public bool HasAllRoles(params string[] roleNames) =>
        roleNames.All(HasRole);

    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        return Task.FromResult(false);
    }

    private record TokenResponse(string Token, DateTime ExpiresAt, int UserId);

    private record UserProfileResponse(
        int Id, string Email, string FirstName, string LastName, DateTime CreatedAt);

    private record ApiErrorResponse(string Error, string Message);
}
