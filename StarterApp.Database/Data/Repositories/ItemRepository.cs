using System.Net;
using System.Net.Http.Json;
using StarterApp.Database.Models;

namespace StarterApp.Database.Data.Repositories;

/// <summary>
/// Implementation of IItemRepository using the shared REST API.
/// Provides API-based persistence for items and categories.
/// </summary>
public class ItemRepository : IItemRepository
{
    private readonly HttpClient _httpClient;

    public ItemRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ==================== Item Operations ====================

    /// <inheritdoc/>
    public async Task<List<Item>> GetAllItemsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("items");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ItemsResponse>();

            return result?.Items ?? new List<Item>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Item>> GetNearbyItemsAsync(
        double latitude,
        double longitude,
        double radiusKm = 5,
        string? category = null)
    {
        try
        {
            var url = $"items/nearby?lat={latitude}&lon={longitude}&radius={radiusKm}";

            if (!string.IsNullOrWhiteSpace(category))
            {
                url += $"&category={Uri.EscapeDataString(category)}";
            }

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new List<Item>();
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NearbyItemsResponse>();

            return result?.Items ?? new List<Item>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading nearby items: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Item?> GetItemByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"items/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Item>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading item {id}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Item?> CreateItemAsync(Item item)
    {
        try
        {

            var payload = new
            {
                title = item.Title,
                description = item.Description,
                dailyRate = item.DailyRate,
                categoryId = item.CategoryId,
                latitude = item.Latitude,
                longitude = item.Longitude
            };

            var response = await _httpClient.PostAsJsonAsync("items", payload);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Item>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating item: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Item?> UpdateItemAsync(Item item)
    {
        try
        {

            var payload = new
            {
                title = item.Title,
                description = item.Description,
                dailyRate = item.DailyRate,
                isAvailable = item.IsAvailable
            };

            var response = await _httpClient.PutAsJsonAsync($"items/{item.Id}", payload);

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Item>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating item {item.Id}: {ex.Message}");
            throw;
        }
    }

    // ==================== Category Operations ====================

    /// <inheritdoc/>
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("categories");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CategoriesResponse>();

            return result?.Categories ?? new List<Category>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading categories: {ex.Message}");
            throw;
        }
    }

    // ==================== Helpers ====================

// These private classes match the JSON wrapper objects returned by the API.
// The API does not always return a plain List<Item> directly. For example,
// /items returns { items, totalItems, page, pageSize, totalPages } and
// /items/nearby returns { items, searchLocation, radius, totalResults }.
// These helper classes let ReadFromJsonAsync deserialize that response shape,
// then the repository returns only the useful List<Item> or List<Category>
// back to the ViewModels.

    private class ItemsResponse
    {
        public List<Item> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

        private class NearbyItemsResponse
    {
        public List<Item> Items { get; set; } = new();
        public SearchLocationResponse? SearchLocation { get; set; }
        public double Radius { get; set; }
        public int TotalResults { get; set; }
    }

    private class SearchLocationResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    private class CategoriesResponse
    {
        public List<Category> Categories { get; set; } = new();
    }
}