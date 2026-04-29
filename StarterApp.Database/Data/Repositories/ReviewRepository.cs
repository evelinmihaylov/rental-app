using System.Net;
using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Implementation of IReviewRepository using the shared REST API.
/// Provides API-based persistence for reviews.
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly HttpClient _httpClient;

    public ReviewRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ==================== Review Operations ====================

    /// <inheritdoc/>
    public async Task<Review?> CreateReviewAsync(int rentalId, int rating, string? comment)
    {
        try
        {
            var payload = new
            {
                rentalId = rentalId,
                rating = rating,
                comment = comment
            };

            var response = await _httpClient.PostAsJsonAsync("reviews", payload);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Failed to create review.");

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException(message);
                }

                if (response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new InvalidOperationException(message);
                }

                throw new InvalidOperationException(message);
            }

            return await response.Content.ReadFromJsonAsync<Review>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating review: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ReviewListResult> GetItemReviewsAsync(int itemId, int page = 1, int pageSize = 10)
    {
        try
        {
            var endpoint = $"items/{itemId}/reviews?page={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    $"Failed to load reviews for item {itemId}.");

                throw new InvalidOperationException(message);
            }

            return await response.Content.ReadFromJsonAsync<ReviewListResult>()
                ?? new ReviewListResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reviews for item {itemId}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ReviewListResult> GetUserReviewsAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var endpoint = $"users/{userId}/reviews?page={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    $"Failed to load reviews for user {userId}.");

                throw new InvalidOperationException(message);
            }

            return await response.Content.ReadFromJsonAsync<ReviewListResult>()
                ?? new ReviewListResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reviews for user {userId}: {ex.Message}");
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

    private record ApiErrorResponse(string Error, string Message, object? Details);
}