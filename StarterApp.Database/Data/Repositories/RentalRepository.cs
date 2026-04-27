using System.Net;
using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Implementation of IRentalRepository using the shared REST API.
/// Provides API-based persistence for rentals.
/// </summary>
public class RentalRepository : IRentalRepository
{
    private readonly HttpClient _httpClient;

    public RentalRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ==================== Rental Operations ====================

    /// <inheritdoc/>
    public async Task<Rental?> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        try
        {

            var payload = new
            {
                itemId = itemId,
                startDate = startDate.ToString("yyyy-MM-dd"),
                endDate = endDate.ToString("yyyy-MM-dd")
            };

            var response = await _httpClient.PostAsJsonAsync("rentals", payload);

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Conflict ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Rental>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating rental request: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Rental>> GetIncomingRentalsAsync(string? status = null)
    {
        try
        {

            var endpoint = "rentals/incoming";

            if (!string.IsNullOrWhiteSpace(status))
            {
                endpoint += $"?status={Uri.EscapeDataString(status)}";
            }

            var response = await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RentalsResponse>();

            return result?.Rentals ?? new List<Rental>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading incoming rentals: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Rental>> GetOutgoingRentalsAsync(string? status = null)
    {
        try
        {

            var endpoint = "rentals/outgoing";

            if (!string.IsNullOrWhiteSpace(status))
            {
                endpoint += $"?status={Uri.EscapeDataString(status)}";
            }

            var response = await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RentalsResponse>();

            return result?.Rentals ?? new List<Rental>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading outgoing rentals: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Rental?> GetRentalByIdAsync(int id)
    {
        try
        {

            var response = await _httpClient.GetAsync($"rentals/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Rental>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rental {id}: {ex.Message}");
            throw;
        }
    }

     /// <inheritdoc/>
    public async Task<Rental?> UpdateRentalStatusAsync(int rentalId, string status)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Patch,
                $"rentals/{rentalId}/status")
            {
                Content = JsonContent.Create(new { status })
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Failed to update rental status.");

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException(message);
                }

                throw new InvalidOperationException(message);
            }

            return await response.Content.ReadFromJsonAsync<Rental>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating rental {rentalId} status: {ex.Message}");
            throw;
        }
    }

    // ==================== Helpers ====================
    private async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallbackMessage)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return !string.IsNullOrWhiteSpace(error?.Message)
                ? error.Message
                : fallbackMessage;
        }
        catch
        {
            return fallbackMessage;
        }
    }

    private class RentalsResponse
    {
        public List<Rental> Rentals { get; set; } = new();
        public int TotalRentals { get; set; }
    }
    
    private record ApiErrorResponse(string Error, string Message);
}